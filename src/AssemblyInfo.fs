namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ebs-backup")>]
[<assembly: AssemblyProductAttribute("Exira.EbsBackup")>]
[<assembly: AssemblyDescriptionAttribute("Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule")>]
[<assembly: AssemblyVersionAttribute("1.4.6")>]
[<assembly: AssemblyFileVersionAttribute("1.4.6")>]
[<assembly: AssemblyMetadataAttribute("githash","47dc7378ae9e859d0388489ce0c8ef4164c0086a")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.4.6"
