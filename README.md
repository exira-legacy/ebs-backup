# ebs-backup [![NuGet Status](http://img.shields.io/nuget/v/Exira.EbsBackup.svg?style=flat)](https://www.nuget.org/packages/Exira.EbsBackup/)

## Exira.EbsBackup

Exira.EbsBackup is a console application which backs up EBS volumes on a rotating schedule.

## Usage

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

## Cloning

```git clone git@github.com:exira/ebs-backup.git -c core.autocrlf=input```

## Copyright

Copyright © 2015 Cumps Consulting BVBA / Exira and contributors.

## License

ebs-backup is licensed under [BSD (3-Clause)](http://choosealicense.com/licenses/bsd-3-clause/ "Read more about the BSD (3-Clause) License"). Refer to [LICENSE.txt](https://github.com/exira/ebs-backup/blob/master/LICENSE.txt) for more information.
