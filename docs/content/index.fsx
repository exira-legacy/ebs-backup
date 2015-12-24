(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
ebs-backup [![NuGet Status](http://img.shields.io/nuget/v/Exira.EbsBackup.svg?style=flat)](https://www.nuget.org/packages/Exira.EbsBackup/)
======================

Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule.

### Usage

 * Tag the volumes you wish to backup in the AWS console, e.g.: `backup` (a value is not needed)

 * Download the latest release from [the GitHub releases page](https://github.com/exira/ebs-backup/releases)
 * Unzip somewhere on a machine where you can put it in a scheduled task

 * Edit `Backup.yaml` using:
  * AwsAccessKey: `YOUR AWS ACCESS KEY`, e.g.: `AXIURT98PXVXJU7K...`
  * AwsSecretKey: `YOUR AWS SECRET KEY`, e.g.: `1Y9poTREdb/8j1234Tr...`
  * Region: `YOUR AWS REGION`, e.g.: `eu-central-1`
  * NumberOfDailyBackups: `# daily backups to keep`, e.g.: `7`
  * NumberOfWeeklyBackups: `# weekly backups to keep`, e.g.: `4`
  * NumberOfMonthlyBackups: `# monthly backups to keep`, e.g.: `6`
  * NumberOfYearlyBackups: `# yearly backups to keep`, e.g.: `2`
  * Tags: `List of tagnames`, e.g.: `- backup`

 * Run `ebs-backup.exe` on a fixed time (put it in a scheduled task)

 * For all volumes with the specified tags a snapshot will be taken and snapshots which do not meet your retention settings will be deleted

### Cloning

```git clone git@github.com:exira/ebs-backup.git -c core.autocrlf=input```

### Contributing and copyright

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork
the project and submit pull requests. You might also want to read the
[library design notes][readme] to understand how it works.

For more information see the [License file][license] in the GitHub repository.

  [content]: https://github.com/exira/ebs-backup/tree/master/docs/content
  [gh]: https://github.com/exira/ebs-backup
  [issues]: https://github.com/exira/ebs-backup/issues
  [readme]: https://github.com/exira/ebs-backup/blob/master/README.md
  [license]: https://github.com/exira/ebs-backup/blob/master/LICENSE.txt
*)
