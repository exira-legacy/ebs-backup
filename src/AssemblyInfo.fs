namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ebs-backup")>]
[<assembly: AssemblyProductAttribute("Exira.EbsBackup")>]
[<assembly: AssemblyDescriptionAttribute("Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule")>]
[<assembly: AssemblyVersionAttribute("1.5.7")>]
[<assembly: AssemblyFileVersionAttribute("1.5.7")>]
[<assembly: AssemblyMetadataAttribute("githash","eb64295e1a8bf2fb70af75ded7d189a0ef9ede0d")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.5.7"
