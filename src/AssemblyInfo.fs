namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("route-53-updater")>]
[<assembly: AssemblyProductAttribute("Exira.Route53Updater")>]
[<assembly: AssemblyDescriptionAttribute("Exira.Route53Updater is a console application which updates a Route 53 record with the current EC2 hostname")>]
[<assembly: AssemblyVersionAttribute("1.1.12")>]
[<assembly: AssemblyFileVersionAttribute("1.1.12")>]
[<assembly: AssemblyMetadataAttribute("githash","1255678a52f1a8535e8d93e72aaaed0ab49409c5")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.1.12"
