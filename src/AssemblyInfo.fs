namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ebs-backup")>]
[<assembly: AssemblyProductAttribute("Exira.EbsBackup")>]
[<assembly: AssemblyDescriptionAttribute("Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule")>]
[<assembly: AssemblyVersionAttribute("1.2.2")>]
[<assembly: AssemblyFileVersionAttribute("1.2.2")>]
[<assembly: AssemblyMetadataAttribute("githash","d80733ee88d11bbc43728c4c4bd7bfd48d464a24")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.2.2"
