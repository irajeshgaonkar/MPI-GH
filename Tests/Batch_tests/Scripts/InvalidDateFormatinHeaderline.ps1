param(
    [ValidateSet('dev', 'int')]
    [string]$Environment = 'dev'
)

$root = Split-Path $PSScriptRoot -Parent
Import-Module (Join-Path $root 'PSmodule\s3Handler.psm1') -Force
$config = Import-PowerShellDataFile (Join-Path $root "config\$Environment.psd1")

$sourceCsv = Join-Path $root 'TestData\doh.wadoh.wdrs.csv'
$csvPath = Join-Path $root "TestData\ModifyDateFormat.csv"
Modify-DateFormat -localFilePath $sourceCsv -newCsvPath $csvPath

Invoke-BatchCsvTest -CsvPath $csvPath -InputBucket $config.InputBucket -OutputBucket $config.OutputBucket -AwsProfile $config.AwsProfile -DownloadPath (Join-Path $root 'ProcessedFiles') -NoOutputMessage 'No output processed file was generated, as expected, since the file has invalid date format.'
