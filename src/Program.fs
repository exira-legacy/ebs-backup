open FSharp.Configuration
open System
open System.IO
open System.Reflection
open Amazon
open Amazon.EC2
open Amazon.EC2.Model
open Exira.ErrorHandling

let executablePath = Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let configPath = Path.Combine(executablePath, "Backup.yaml")

type BackupConfig = YamlConfig<"Backup.yaml">
let backupConfig = BackupConfig()
backupConfig.Load configPath

type Errors =
    | FailedToFindVolume of string
    | FailedToRetrieveVolume of string * exn
    | FailedToTakeSnapshot of string * exn
    | FailedToRetrieveSnapshots of string * exn
    | FailedToDeleteSnapshot of string * exn
    | FailedToDeleteSnapshots of seq<Errors> * seq<Snapshot>

let format errors =
    let rec formatError error =
        match error with
        | FailedToFindVolume volume -> sprintf "Could not find volume %s" volume
        | FailedToRetrieveVolume (volume, ex) -> sprintf "Could not retrieve volume %s: %s" volume (ex.ToString())
        | FailedToTakeSnapshot (volume, ex) -> sprintf "Could not take snapshots for %s: %s" volume (ex.ToString())
        | FailedToRetrieveSnapshots (volume, ex) -> sprintf "Could not retrieve snapshots for %s: %s" volume (ex.ToString())
        | FailedToDeleteSnapshot (snapshot, ex) -> sprintf "Could not delete snapshot %s: %s" snapshot (ex.ToString())
        | FailedToDeleteSnapshots (e, success) ->
            e |> Seq.map formatError |> String.concat "\n"

    errors
    |> List.map formatError

let ec2Client =
    new AmazonEC2Client(
        backupConfig.Backup.AwsAccessKey,
        backupConfig.Backup.AwsSecretKey,
        RegionEndpoint.GetBySystemName backupConfig.Backup.Region)

let printTags (tags: seq<Tag>) =
    tags |> Seq.map (fun t -> sprintf "[%s: %s]" t.Key t.Value) |> String.concat " "

let printVolume (volume: Volume) =
    if backupConfig.Backup.Debug then
        printfn "Volume: %s - Tags: %s" volume.VolumeId (printTags volume.Tags)
    else ()

let printSnapshot message (snapshot: Snapshot) =
    if backupConfig.Backup.Debug then
        printfn "[%s] Volume: %s - Snapshot: %s - Date: %A - Description: %s - Tags: %s" message snapshot.VolumeId snapshot.SnapshotId snapshot.StartTime snapshot.Description (printTags snapshot.Tags)
    else ()

let printSnapshots message snapshots =
    if backupConfig.Backup.Debug then snapshots |> Seq.iter (printSnapshot message)
    else ()

let listVolume volume =
    try
        let request = DescribeVolumesRequest(new System.Collections.Generic.List<string>([ volume ]))
        let response = ec2Client.DescribeVolumes request
        let ebsVolume = response.Volumes |> Seq.tryHead

        match ebsVolume with
        | Some v -> succeed v
        | None -> fail [FailedToFindVolume volume]
    with
    | ex -> fail [FailedToRetrieveVolume (volume, ex)]

let takeSnapshot (volume: Volume) =
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

        succeed snapshot
    with
    | ex -> fail [FailedToTakeSnapshot (volume.VolumeId, ex)]

let getSnapshots (snapshot: Snapshot) =
    try
        let filters = [ Filter("volume-id", new System.Collections.Generic.List<string>([ snapshot.VolumeId ])) ]
        let request = DescribeSnapshotsRequest(Filters = new System.Collections.Generic.List<Filter>(filters))
        let response = ec2Client.DescribeSnapshots request
        succeed response.Snapshots
    with
    | ex -> fail [FailedToRetrieveSnapshots (snapshot.VolumeId, ex)]

let daysToKeep =
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

let pruneSnapshots (snapshots: seq<Snapshot>) =
    let deleteSnapshot (snapshot: Snapshot) =
        try
            let request = DeleteSnapshotRequest snapshot.SnapshotId
            ec2Client.DeleteSnapshot request |> ignore

            succeed snapshot
        with
        | ex -> fail [FailedToDeleteSnapshot (snapshot.SnapshotId, ex)]

    let prunedSnapshots =
        snapshots
        |> Seq.filter (fun s -> daysToKeep |> Seq.contains s.StartTime.Date |> not)
        |> Seq.map deleteSnapshot

    let failures = prunedSnapshots |> Seq.choose failureOnly |> Seq.collect id
    let success = prunedSnapshots |> Seq.choose successOnly

    if Seq.length failures > 0 then fail [FailedToDeleteSnapshots(failures, success)]
    else succeed success

let backup volume =
    volume
    |> listVolume
    |> map (printVolume |> tee)
    |> bind takeSnapshot
    |> map (printSnapshot "Snapshot taken" |> tee)
    |> bind getSnapshots
    |> map (printSnapshots "Snapshots retrieved" |> tee)
    |> bind pruneSnapshots
    |> map (printSnapshots "Snapshots pruned" |> tee)

[<EntryPoint>]
let main _ =
    let backups =
        backupConfig.Backup.Volumes
        |> Seq.map backup

    let hasFailures = Seq.exists isFailure backups

    if hasFailures then
        printfn "Errors:"
        backups
        |> Seq.choose failureOnly
        |> Seq.iter (fun errors ->  errors |> format |> String.concat Environment.NewLine |> printfn "%s")
    else ()

    if hasFailures then -1
    else 0
