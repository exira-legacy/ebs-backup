namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ebs-backup")>]
[<assembly: AssemblyProductAttribute("Exira.EbsBackup")>]
[<assembly: AssemblyDescriptionAttribute("Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule")>]
[<assembly: AssemblyVersionAttribute("1.3.4")>]
[<assembly: AssemblyFileVersionAttribute("1.3.4")>]
[<assembly: AssemblyMetadataAttribute("githash","562c35d05aaf431e09b89ab353515b0b3f03dce2")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.3.4"
