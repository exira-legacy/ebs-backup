namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ebs-backup")>]
[<assembly: AssemblyProductAttribute("Exira.EbsBackup")>]
[<assembly: AssemblyDescriptionAttribute("Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule")>]
[<assembly: AssemblyVersionAttribute("1.2.3")>]
[<assembly: AssemblyFileVersionAttribute("1.2.3")>]
[<assembly: AssemblyMetadataAttribute("githash","26c2d4222ee2d798017710c7482ab6ca984145af")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.2.3"
