namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ebs-backup")>]
[<assembly: AssemblyProductAttribute("Exira.EbsBackup")>]
[<assembly: AssemblyDescriptionAttribute("Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule")>]
[<assembly: AssemblyVersionAttribute("1.3.5")>]
[<assembly: AssemblyFileVersionAttribute("1.3.5")>]
[<assembly: AssemblyMetadataAttribute("githash","867793a7fc391b1d53d4cf9c968ce2c54be588fb")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.3.5"
