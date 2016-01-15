namespace Exira.EbsBackup

module internal Configuration =
    open System.IO
    open System.Reflection
    open FSharp.Configuration

    let private entryAssembly = Assembly.GetEntryAssembly()
    let private executablePath = entryAssembly.Location |> Path.GetDirectoryName
    let private configPath = Path.Combine(executablePath, "Backup.yaml")

    type BackupConfig = YamlConfig<"Backup.yaml">
    let backupConfig = BackupConfig()
    backupConfig.LoadAndWatch configPath |> ignore
