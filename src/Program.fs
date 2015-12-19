open FSharp.Configuration
open System
open System.Reflection
open System.IO
open Amazon.EC2.Util
open Amazon
open Amazon.Runtime
open Exira.ErrorHandling

let executablePath = Assembly.GetEntryAssembly().Location |> Path.GetDirectoryName
let configPath = Path.Combine(executablePath, "Backup.yaml")

type BackupConfig = YamlConfig<"Backup.yaml">
let backupConfig = BackupConfig()
backupConfig.Load configPath

type Errors =
    | FailedToRetrieveEc2MetaData of exn
    | FailedToRetrieveNetworkInterface
    | FailedToRetrieveRoute53Zone of exn
    | FailedToRetrieveRoute53ResourceRecordSet
    | FailedToRetrieveRoute53ResourceRecord
    | Route53ResourceRecordUpToDate
    | FailedToUpdateRoute53Zone of exn

let format errors =
    let formatError error =
        match error with
        | FailedToRetrieveEc2MetaData _ -> sprintf "Could not retrieve EC2 meta data."
        | FailedToRetrieveNetworkInterface -> sprintf "No network interfaces were found."
        | FailedToRetrieveRoute53Zone _ -> sprintf "Could not retrieve Route 53 zone."
        | FailedToRetrieveRoute53ResourceRecordSet -> sprintf "Route 53 Resource Record Set was not found."
        | FailedToRetrieveRoute53ResourceRecord -> sprintf "Route 53 Resource Record was not found."
        | Route53ResourceRecordUpToDate -> sprintf "Route 53 Resource Record is up to date!"
        | FailedToUpdateRoute53Zone _ -> sprintf "Could not update Route 53 zone."

    errors
    |> List.map formatError

[<EntryPoint>]
let main argv =

    0
