namespace Exira.EbsBackup

module Program =
    open System
    open Amazon
    open Amazon.EC2
    open Amazon.EC2.Model
    open Exira.ErrorHandling

    let private backupConfig = Configuration.backupConfig
    let private logger = Serilogger.logger

    type Errors =
        | FailedToBackupVolumes of Errors list
        | FailedToRetrieveVolumes of seq<string> * exn
        | FailedToFindVolume of string
        | FailedToRetrieveVolume of string * exn
        | FailedToTakeSnapshot of string * exn
        | FailedToRetrieveSnapshots of string * exn
        | FailedToDeleteSnapshot of string * exn
        | SnapshotIsInUse of string * exn
        | FailedToDeleteSnapshots of Errors list

    let private format errors =
        let rec formatError error =
            match error with
            | FailedToRetrieveVolumes (tags, ex) -> sprintf "Could not find volumes for [%s]: %s" (String.concat ", " tags) (ex.ToString())
            | FailedToFindVolume volume -> sprintf "Could not find volume %s" volume
            | FailedToRetrieveVolume (volume, ex) -> sprintf "Could not retrieve volume %s: %s" volume (ex.ToString())
            | FailedToTakeSnapshot (volume, ex) -> sprintf "Could not take snapshots for %s: %s" volume (ex.ToString())
            | FailedToRetrieveSnapshots (volume, ex) -> sprintf "Could not retrieve snapshots for %s: %s" volume (ex.ToString())
            | FailedToDeleteSnapshot (snapshot, ex) -> sprintf "Could not delete snapshot %s: %s" snapshot (ex.ToString())
            | SnapshotIsInUse (snapshot, _) -> sprintf "Snapshot %s is in use" snapshot
            | FailedToBackupVolumes e
            | FailedToDeleteSnapshots e
                -> e |> List.map formatError |> String.concat Environment.NewLine

        errors
        |> List.map formatError

    let private printTags (tags: seq<Tag>) =
        tags |> Seq.map (fun t -> sprintf "[%s: %s]" t.Key t.Value) |> String.concat " "

    let private printVolume (volume: Volume) =
        if backupConfig.Backup.Debug then
            printfn "[Volume retrieved] Volume: %s - Tags: %s" volume.VolumeId (printTags volume.Tags)
        else ()

    let private printSnapshot message (snapshot: Snapshot) =
        if backupConfig.Backup.Debug then
            printfn "[%s] Volume: %s - Snapshot: %s - Date: %A - Description: %s - Tags: %s" message snapshot.VolumeId snapshot.SnapshotId snapshot.StartTime snapshot.Description (printTags snapshot.Tags)
        else ()

    let private printSnapshots message (snapshots: seq<Snapshot>) =
        if backupConfig.Backup.Debug then snapshots |> Seq.sortBy (fun s -> s.StartTime) |> Seq.iter (printSnapshot message)
        else ()

    let private ec2Client =
        new AmazonEC2Client(
            backupConfig.Backup.AwsAccessKey,
            backupConfig.Backup.AwsSecretKey,
            RegionEndpoint.GetBySystemName backupConfig.Backup.Region)

    let private findVolumes (tags: seq<string>) =
        try
            let filters = new System.Collections.Generic.List<Filter>([ Filter("tag-key", new System.Collections.Generic.List<string>(tags)) ])
            let request = DescribeVolumesRequest(Filters = filters)
            let response = ec2Client.DescribeVolumes request
            succeed response.Volumes
        with
        | ex -> fail [FailedToRetrieveVolumes (tags, ex)]

    let private listVolume volume =
        try
            let request = DescribeVolumesRequest(new System.Collections.Generic.List<string>([ volume ]))
            let response = ec2Client.DescribeVolumes request
            let ebsVolume = response.Volumes |> Seq.tryHead

            match ebsVolume with
            | Some v -> succeed v
            | None -> fail [FailedToFindVolume volume]
        with
        | ex -> fail [FailedToRetrieveVolume (volume, ex)]

    let private takeSnapshot (volume: Volume) =
        try
            let request = CreateSnapshotRequest(volume.VolumeId, "Automated snapshot taken by ebs-backup")
            let response = ec2Client.CreateSnapshot request
            let snapshot = response.Snapshot

            let name = volume.Tags |> Seq.tryFind (fun t -> t.Key = "Name")
            let name =
                match name with
                | Some tag -> tag.Value
                | None -> volume.VolumeId

            let request =
                CreateTagsRequest(
                    new System.Collections.Generic.List<string>([ snapshot.SnapshotId ]),
                    new System.Collections.Generic.List<Tag>([ Tag("Name", name) ]))
            ec2Client.CreateTags request |> ignore

            logger.Information("EBS volume {name}@{volume} backed up", name, volume.VolumeId)
            succeed snapshot
        with
        | ex -> fail [FailedToTakeSnapshot (volume.VolumeId, ex)]

    let private getSnapshots (snapshot: Snapshot) =
        try
            let filters = [ Filter("volume-id", new System.Collections.Generic.List<string>([ snapshot.VolumeId ])) ]
            let request = DescribeSnapshotsRequest(Filters = new System.Collections.Generic.List<Filter>(filters))
            let response = ec2Client.DescribeSnapshots request
            succeed response.Snapshots
        with
        | ex -> fail [FailedToRetrieveSnapshots (snapshot.VolumeId, ex)]

    let private daysToKeep =
        let today = DateTime.Today
        let numberOfDailyBackups = backupConfig.Backup.NumberOfDailyBackups
        let numberOfWeeklyBackups = backupConfig.Backup.NumberOfWeeklyBackups
        let numberOfMonthlyBackups = backupConfig.Backup.NumberOfMonthlyBackups
        let numberOfYearlyBackups = backupConfig.Backup.NumberOfYearlyBackups

        let firstOfMonth = DateTime(today.Year, today.Month, 1)
        let firstOfYear = DateTime(today.Year, 1, 1)

        let daily = seq { for n in 0 .. numberOfDailyBackups - 1 do yield today.AddDays (float n * -1.) }

        let weekly =
            Seq.initInfinite (fun i -> today.AddDays (float i * -1.))
            |> Seq.filter (fun date -> date.DayOfWeek = DayOfWeek.Sunday)
            |> Seq.take numberOfWeeklyBackups

        let monthly = seq { for n in 0 .. numberOfMonthlyBackups - 1 do yield firstOfMonth.AddMonths (n * -1) }

        let yearly = seq { for n in 0 .. numberOfYearlyBackups - 1 do yield firstOfYear.AddYears (n * -1) }

        daily
        |> Seq.append weekly
        |> Seq.append monthly
        |> Seq.append yearly
        |> Seq.distinct
        |> Seq.toList

    let private pruneSnapshots (snapshots: seq<Snapshot>) =
        let deleteSnapshot (snapshot: Snapshot) =
            try
                let request = DeleteSnapshotRequest snapshot.SnapshotId
                ec2Client.DeleteSnapshot request |> ignore

                logger.Information("EBS snapshot {snapshot} deleted", snapshot.SnapshotId)
                succeed snapshot
            with
            | ex ->
                if ex.Message.Contains "is currently in use by" then fail [SnapshotIsInUse (snapshot.SnapshotId, ex)]
                else fail [FailedToDeleteSnapshot (snapshot.SnapshotId, ex)]

        let prunedSnapshots =
            snapshots
            |> Seq.filter (fun s -> daysToKeep |> List.contains s.StartTime.Date |> not)
            |> Seq.map deleteSnapshot
            |> Seq.toList

        let failures = prunedSnapshots |> List.choose failureOnly |> List.collect id
        let success = prunedSnapshots |> List.choose successOnly

        if List.isEmpty failures then
            succeed success
        else
            printSnapshots "Snapshots pruned" success
            fail [FailedToDeleteSnapshots failures]

    let private backupVolume (volume: Volume) =
        volume.VolumeId
        |> listVolume
        |> map (printVolume |> tee)
        |> bind takeSnapshot
        |> map (printSnapshot "Snapshot taken" |> tee)
        |> bind getSnapshots
        |> map (printSnapshots "Snapshots retrieved" |> tee)
        |> bind pruneSnapshots
        |> map (printSnapshots "Snapshots pruned" |> tee)

    let private backup (volumes: seq<Volume>) =
        let backups =
            volumes
            |> Seq.map backupVolume
            |> Seq.toList

        let failures = backups |> List.choose failureOnly |> List.collect id
        let success = backups |> List.choose successOnly |> Seq.collect id

        if List.isEmpty failures then succeed success
        else fail [FailedToBackupVolumes failures]

    let private backupFailed errors =
        let errors =
            errors
            |> format
            |> String.concat Environment.NewLine
            |> sprintf "Errors:%s %s" Environment.NewLine

        printfn "%s" errors
        logger.Fatal("EBS backup failed :( {errors}", errors)

    [<EntryPoint>]
    let main _ =
        let backups =
            backupConfig.Backup.Tags
            |> findVolumes
            |> bind backup

        match backups with
        | Success _ -> 0
        | Failure errors ->
            backupFailed errors
            -1
