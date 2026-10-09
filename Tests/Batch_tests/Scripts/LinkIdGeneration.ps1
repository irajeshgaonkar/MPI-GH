param(
    [ValidateSet('dev', 'int')]
    [string]$Environment = 'dev'
)

$root = Split-Path $PSScriptRoot -Parent
Import-Module (Join-Path $root 'PSmodule\s3Handler.psm1') -Force
$config = Import-PowerShellDataFile (Join-Path $root "config\$Environment.psd1")

$csvPath = Join-Path $root 'TestData\doh.wadoh.wdrs.csv'

Invoke-BatchCsvTest -CsvPath $csvPath -InputBucket $config.InputBucket -OutputBucket $config.OutputBucket -AwsProfile $config.AwsProfile -DownloadPath (Join-Path $root 'ProcessedFiles') -ExpectProcessedFile
