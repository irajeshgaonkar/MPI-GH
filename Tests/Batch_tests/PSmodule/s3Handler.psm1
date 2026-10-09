function Upload-CsvToS3 {
    param (
        [string]$localFilePath,
        [string]$s3BucketName,
        [Parameter(Mandatory = $true)]
        [string]$awsProfile
    )
    
    # Generate timestamp in yyyyMMDD.HHMMSS format
    $timeStamp = [Datetime]::Now.ToString("yyyyMMdd.HHmmss")

    # Extract filename and extension
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($localFilePath)
    $fileExtension = [System.IO.Path]::GetExtension($localFilePath)

    # Create a new file name with the timestamp
    $s3FileName = "$fileName.$timeStamp$fileExtension"

    # Upload the file to the S3 bucket
    aws s3 cp $localFilePath "s3://$s3BucketName/$s3FileName" --profile $awsProfile

    # Verify if the upload was successful
    if ($LASTEXITCODE -eq 0) {
        Write-Host "File uploaded successfully to S3!"
    } else {
        Write-Host "Failed to upload the file to S3."
    }

    
    return @{ UploadedFile = $s3FileName; TimeStamp = $timeStamp }
}


function Update-CsvText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LocalFilePath,
        [Parameter(Mandatory = $true)]
        [string]$NewCsvPath,
        [Parameter(Mandatory = $true)]
        [array]$Replacements
    )

    Write-Host "Reading CSV from: $LocalFilePath"

    if (-not (Test-Path $LocalFilePath)) {
        Write-Host "Error: The file '$LocalFilePath' does not exist."
        return
    }

    # Replacements run in order. A later value can match text inserted by an earlier one.
    $content = Get-Content -Path $LocalFilePath
    foreach ($replacement in $Replacements) {
        $content = $content.Replace($replacement.Old, $replacement.New)
    }

    Set-Content -Value $content -Path $NewCsvPath
    Write-Host "CSV file saved to: $NewCsvPath"
}
function DuplicateSourceID {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'WKKHLS0ME44Q1'; New = 'DVBCHQ2RG41R1' }
    )
}

function InvalidBirthYear {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '6/4/1950'; New = '6/4/2050' }
    )
}

function InvalidBirthDateFormat {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '11/21/1947'; New = '21/11/1947' }
    )
}

function partialDateTimeHeaderline {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '1/17/2023'; New = '1/17' }
        @{ Old = '5:06:00 AM'; New = '5:06' }
    )
}

function ExceedsCharacterLimitDEFH {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'DOMITILA'; New = 'BGHJKILOPYGTREDFRSCXVZ456TGF2SDCVFRGTYUI89' }
        @{ Old = 'CATHY'; New = 'BGHJKILOPYGTREDFRSCXVZ456TGF2SDCVFRGTYUI89' }
        @{ Old = 'ROSE'; New = 'BGHJKILOPYGTREDFRSCXVZ456TGF2SDCVFRGTYUI89' }
        @{ Old = '2/12/1975'; New = '2/12/1987HJUIK5678902WSDFGTHY678UJNBVCXZDEFRS5678912' }
    )
}

function ExceedsCharacterLimitIJ {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'F'; New = 'Binaryyhsdg' }
        @{ Old = '702596499'; New = '729091qwuj789' }
    )
}

function ExceedsCharacterLimitLOPQTV {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '165 PRIVATE ROAD 21050'; New = 'hyjukiwsvbnmjiklo234567890olkijhuygfdsawqertyuiomnbvcxzasdfghjklpoiuytrewq1234567890oplkiujyhtgrfedwsqazaxscdvfbgnhmjlkpoiuytrfdewsqazxcsdfvbgnhmjklpoiuytrewqazaxscdvfbgnhmjklpoiuytgrfedwsqazaxscdvfbgnhmjklpo09i8u7y6t5r4e3w2q1q2w3e4r5t6y7u8i9ooooplkjhgfdsaqwertyu' }
        @{ Old = 'GREEN MOUNTAIN'; New = 'hyjukiwsvbnmjiklo234567890olkijhuygfdsawqertyuiomnbvcxzasdfghjklpoiuytrewq1234567890oplkiujyhtgrfedwsq' }
        @{ Old = 'KS'; New = 'BGHJKILOPYGTREDFRSCXVZ456TGF2SDCVFRGTYUI89' }
        @{ Old = '85747'; New = 'dfrgt6y' }
        @{ Old = '9104965689'; New = 'wsdefrg1234567890okij' }
        @{ Old = 'DRMESA@GMAIL.COM'; New = 'aswderftg23456yh7ujkiolp098mnbvcxzasdfqgrfgthy' }
        @{ Old = 'Home'; New = 'edfrgthyjukilopa3245678901rfsvgxbhsnjmklzoiuj' }
    )
}

function NonnumericSSN {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '106861939'; New = 'edfrgthuj' }
    )
}

function partialDateTime {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '11/14/2022 3:32'; New = '11/14/ 3:' }
    )
}

function GenderWithSplCharacter {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'M'; New = 'M@!#' }
    )
}

function invalidCharctersInDateTime {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '1/12/2023 7:54'; New = '1/12/2023 7:AP' }
    )
}

function BlankAPICall {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'VE Post'; New = ' ' }
    )
}

function LeadingTrailingSpaceGender {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'F'; New = ' F ' }
    )
}

function LeadingTrailingSpaceDEFHKLOPQSTUV {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'DION'; New = ' DION ' }
        @{ Old = 'TERRENCE'; New = ' TERRENCE ' }
        @{ Old = 'PFEIFER'; New = ' PFEIFER ' }
        @{ Old = '5/6/1999'; New = ' 5/6/1999 ' }
        @{ Old = 'WORK'; New = ' WORK ' }
        @{ Old = '1753 PO BOX'; New = ' 1753 PO BOX ' }
        @{ Old = 'MANCHESTER'; New = ' MANCHESTER ' }
        @{ Old = 'TX'; New = ' TX ' }
        @{ Old = '40165'; New = ' 40165 ' }
        @{ Old = '9104965689'; New = ' 9104965689 ' }
        @{ Old = 'GNROSE@GMAIL.COM'; New = ' GNROSE@GMAIL.COM ' }
    )
}

function SSNWithSplCharacter {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '716978349'; New = '7169&$78349' }
    )
}

function LeadingTrailingSpaceSSN {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '830875391'; New = ' 830875391 ' }
    )
}

function Remove-DateFromCsv {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '1/17/2023'; New = ' ' }
    )
}

function Remove-MandatoryFieldsFromCsv {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'PEEJKW6ME91Y1'; New = ' ' }
    )
}

function IncorrectDateTimeofSource {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '10/29/2022 20:54'; New = '29/10/2022 19:35:12' }
    )
}

function ExpandLimit {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'DVBCHQ2RG41R1'; New = 'DFG6HUJNVXBHJNMKLIOP098HGDFRA4SGTHY678JUYGHNBFREWSQADFGHJKLP09876YHTG4FDE321SWAZXCVFRGTHNBMKLOPIUJYHG' }
    )
}

function Remove-OptionalFieldsFromCSV {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '10/12/2022 11:36'; New = ' ' }
        @{ Old = 'SYNTHIA'; New = ' ' }
    )
}

function Default-APICall {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'VE Post'; New = ' ' }
    )
}

function Remove-TimeFromCsv {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '5:06:00'; New = ' ' }
    )
}

function Modify-DateFormat {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '1/17/2023'; New = '2023/17/1' }
    )
}

function Special-Characters {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = 'O'; New = '$%#' }
    )
}

function Incorrect-TrailerCount {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '10'; New = '3' }
    )
}

function Modify-TimeFormat {
    param(
        [string]$localFilePath,
        [string]$newCsvPath
    )

    Update-CsvText -LocalFilePath $localFilePath -NewCsvPath $newCsvPath -Replacements @(
        @{ Old = '5:06:00'; New = '3:56' }
    )
}


function Verify-OutputFile {
    param (
        [string]$fileName,
        [string]$timeStamp,
        [string]$fileExtension,
        [string]$s3OutputBucket,
        [Parameter(Mandatory = $true)]
        [string]$awsProfile
    )

    $s3ProcessedFile = "$fileName.$timeStamp`_processed$fileExtension"

    $fileFound = $false

    while (-not $fileFound) {
        # Allow the system to generate the output file
        Start-Sleep -Seconds 300

        # List the files in the output folder and check for the specific file
        $files = aws s3 ls "s3://$s3OutputBucket" --profile $awsProfile

        if ($files -match $s3ProcessedFile) {
            Write-Host "The file '$s3ProcessedFile' is present in the output folder."
            $fileFound = $true
        } else {
            Write-Host "The file '$s3ProcessedFile' was not found in the output folder. Checking again in 5 minutes."
        }
    }

    # Return the filename
    return $s3ProcessedFile
}

function Verify-OutputLimited {
    param (
        [string]$fileName,
        [string]$timeStamp,
        [string]$fileExtension,
        [string]$s3OutputBucket,
        [Parameter(Mandatory = $true)]
        [string]$awsProfile
    )

    $s3ProcessedFile = "$fileName.$timeStamp`_processed$fileExtension"

    # Allow the system to generate the output file
    Start-Sleep -Seconds 600

    # List the files in the output folder and check for the specific file
    Write-Host "Listing files in S3 output bucket '$s3OutputBucket'..."
    $files = aws s3 ls "s3://$s3OutputBucket" --profile $awsProfile

    Write-Host "Files listed in S3 output bucket:"
    Write-Host $files

    if ($files -match $s3ProcessedFile) {
        Write-Host "The file '$s3ProcessedFile' is present in the output folder."
        return $s3ProcessedFile
    } else {
        Write-Host "The file '$s3ProcessedFile' was not found in the output folder."
        return $null
    }
}



function Download-CsvFromS3 {
    param (
        [string]$s3ProcessedFile,
        [string]$s3OutputBucket,
        [string]$downloadPath,
        [Parameter(Mandatory = $true)]
        [string]$awsProfile
    )

    $outputFile = Join-Path -Path $downloadPath -ChildPath $s3ProcessedFile

    Write-Host "Downloading from s3://$s3OutputBucket/$s3ProcessedFile to $outputFile"

    # Download CSV file from S3 bucket
    aws s3 cp "s3://$s3OutputBucket/$s3ProcessedFile" $outputFile --profile $awsProfile

    # Check if file download was successful
    if (Test-Path $outputFile) {
        Write-Host "File downloaded successfully to $outputFile"
    } else {
        Write-Host "Failed to download the file from S3."
        exit 1 # Exit if the download fails
    }
}

function Verify-LinkId {
    param (
        [string]$outputFile
    )

    Write-Host "Verifying file at path: $outputFile"

    # Read the CSV file and check if DOH column is filled
    if (-not (Test-Path $outputFile)) {
        Write-Host "The specified output file does not exist: $outputFile"
        exit 1 # Exit if the output file does not exist
    }

    $csvData = Import-Csv -Path $outputFile

    # Check if DOH column exists
    if (-not ($csvData[0].PSObject.Properties.Name -contains "doh")) {
        Write-Host "The DOH column does not exist in the CSV file."
        exit 1 # Exit if DOH column does not exist
    }

    # Iterate through the records and check if link ID is present for all records
    $missingLinkId = @()
    foreach ($record in $csvData) {
        if ([string]::IsNullOrWhiteSpace($record.doh)) {
            # Collect records where link ID is missing or empty
            $missingLinkId += $record
        }
    }

    # Report the results
    if ($missingLinkId.Count -eq 0) {
        Write-Host "MPI LinkID is generated for all records!"
    } else {
        Write-Host "Some records are missing link ID. Below are the records with missing link ID:"
        $missingLinkId | ForEach-Object { Write-Host $_ }
    }
}

function Get-BatchOutputMessage {
    param(
        [string]$ProcessedFile,
        [Parameter(Mandatory = $true)]
        [string]$NoOutputMessage,
        [string]$OutputFoundMessage = 'An output processed file was generated unexpectedly:'
    )

    if ([string]::IsNullOrWhiteSpace($ProcessedFile)) {
        return $NoOutputMessage
    }

    return "$OutputFoundMessage $ProcessedFile"
}

function Invoke-BatchCsvTest {
    [CmdletBinding(DefaultParameterSetName = 'ExpectNoOutput')]
    param(
        [Parameter(Mandatory = $true)]
        [string]$CsvPath,
        [Parameter(Mandatory = $true)]
        [string]$InputBucket,
        [Parameter(Mandatory = $true)]
        [string]$OutputBucket,
        [Parameter(Mandatory = $true)]
        [string]$AwsProfile,
        [Parameter(Mandatory = $true)]
        [string]$DownloadPath,
        [Parameter(Mandatory = $true, ParameterSetName = 'ExpectOutput')]
        [switch]$ExpectProcessedFile,
        [Parameter(Mandatory = $true, ParameterSetName = 'ExpectNoOutput')]
        [string]$NoOutputMessage,
        [Parameter(ParameterSetName = 'ExpectNoOutput')]
        [string]$OutputFoundMessage = 'An output processed file was generated unexpectedly:'
    )

    New-Item -ItemType Directory -Force -Path $DownloadPath | Out-Null

    $result = Upload-CsvToS3 -localFilePath $CsvPath -s3BucketName $InputBucket -awsProfile $AwsProfile
    if (-not $result) {
        # A failed upload has no timestamp, so there is nothing to wait for.
        Write-Host "Upload failed, unable to retrieve file name and timestamp."
        return
    }

    $s3FileName = $result.UploadedFile
    $timeStamp = $result.TimeStamp
    $fileExtension = [System.IO.Path]::GetExtension($CsvPath)
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($CsvPath)

    Write-Host "'$s3FileName' uploaded to $InputBucket"
    Write-Host "Timestamp used for verification: '$timeStamp'"
    Write-Host "File Name: $fileName"
    Write-Host "Time Stamp: $timeStamp"
    Write-Host "File Extension: $fileExtension"

    if ($ExpectProcessedFile) {
        $s3ProcessedFile = Verify-OutputFile -fileName $fileName -timeStamp $timeStamp -fileExtension $fileExtension -s3OutputBucket $OutputBucket -awsProfile $AwsProfile
        Download-CsvFromS3 -s3ProcessedFile $s3ProcessedFile -s3OutputBucket $OutputBucket -downloadPath $DownloadPath -awsProfile $AwsProfile
        Verify-LinkId -outputFile (Join-Path -Path $DownloadPath -ChildPath $s3ProcessedFile)
        return
    }

    $s3ProcessedFile = Verify-OutputLimited -fileName $fileName -timeStamp $timeStamp -fileExtension $fileExtension -s3OutputBucket $OutputBucket -awsProfile $AwsProfile
    Write-Host (Get-BatchOutputMessage -ProcessedFile $s3ProcessedFile -NoOutputMessage $NoOutputMessage -OutputFoundMessage $OutputFoundMessage)

    if ($s3ProcessedFile) {
        Download-CsvFromS3 -s3ProcessedFile $s3ProcessedFile -s3OutputBucket $OutputBucket -downloadPath $DownloadPath -awsProfile $AwsProfile
        Verify-LinkId -outputFile (Join-Path -Path $DownloadPath -ChildPath $s3ProcessedFile)
    }
}

Export-ModuleMember -Function 'Upload-CsvToS3', 'DuplicateSourceID', 'InvalidBirthYear', 'InvalidBirthDateFormat', 'partialDateTimeHeaderline', 'ExceedsCharacterLimitDEFH', 'ExceedsCharacterLimitIJ', 'ExceedsCharacterLimitLOPQTV', 'NonnumericSSN', 'partialDateTime', 'GenderWithSplCharacter', 'invalidCharctersInDateTime', 'BlankAPICall', 'LeadingTrailingSpaceGender', 'LeadingTrailingSpaceDEFHKLOPQSTUV', 'SSNWithSplCharacter', 'LeadingTrailingSpaceSSN', 'Remove-DateFromCsv', 'Remove-MandatoryFieldsFromCsv', 'IncorrectDateTimeofSource', 'ExpandLimit', 'Remove-OptionalFieldsFromCSV', 'Default-APICall', 'Remove-TimeFromCsv', 'Modify-DateFormat', 'Special-Characters', 'Incorrect-TrailerCount', 'Modify-TimeFormat', 'Verify-OutputFile', 'Verify-OutputLimited', 'Download-CsvFromS3', 'Verify-LinkId', 'Invoke-BatchCsvTest'
