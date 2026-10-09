# Overview

This PowerShell module contains several functions for processing CSV files, uploading them to AWS S3, and performing various modifications to the data. It allows for file uploads to S3, removal of specific date/time data, format modifications, special character replacements, and validation of file contents.

## Key Features
- Upload CSV to AWS S3 with timestamp-based file naming.
- Modify or remove specific date, time, or characters in CSV files.
- Verify and download processed files from S3.
- Check and validate content, specifically for missing or malformed fields.

## Folder Structure

Shared files live once. Environment settings live in `config`.

### config
`dev.psd1` and `int.psd1` hold the AWS profile and the input and output bucket names.

### PSmodule
`s3Handler.psm1`. CSV helpers are shared. S3 calls take `-awsProfile` from the config file.

### Scripts
One PowerShell script per test case. Pass the environment when you run it:

```powershell
.\Scripts\BlankAPIcall.ps1 -Environment dev
.\Scripts\BlankAPIcall.ps1 -Environment int
```

The default environment is `dev`.

### TestData
Source CSV fixtures. Scripts also write the modified CSV for each test into this folder before upload.

### ProcessedFiles
Files downloaded from the S3 output bucket. Not committed.

  
## Functions

### Upload-CsvToS3
Uploads a local CSV file to an AWS S3 bucket with a timestamp-based filename.
#### Parameters:
localFilePath (string): The path to the local CSV file to upload.

s3BucketName (string): The S3 bucket name where the file will be uploaded.

awsProfile (string, required): The AWS profile to use for the upload. Scripts pass this from `config`.


### Remove-DateFromCsv
Removes a specific date from a CSV file.
#### Parameters:
localFilePath (string): Path to the input CSV file.

newCsvPath (string): Path where the modified CSV file will be saved.


### Default-APICall
Replaces the "VE Post" text in the CSV file with a space.
#### Parameters:
localFilePath (string): Path to the input CSV file.

newCsvPath (string): Path where the modified CSV file will be saved.


###  Remove-TimeFromCsv
Removes a specific time value from the CSV file.
#### Parameters:
localFilePath (string): Path to the input CSV file.

newCsvPath (string): Path where the modified CSV file will be saved.


### Modify-DateFormat
Modifies the date format from MM/DD/YYYY to YYYY/DD/MM.
#### Parameters:
localFilePath (string): Path to the input CSV file.

newCsvPath (string): Path where the modified CSV file will be saved.


### Special-Characters
Replaces all occurrences of the letter "O" with $%# in the CSV file.
#### Parameters:
localFilePath (string): Path to the input CSV file.

newCsvPath (string): Path where the modified CSV file will be saved.


###  Incorrect-TrailerCount
Replaces occurrences of 10 with 3 in the CSV file.
#### Parameters:
localFilePath (string): Path to the input CSV file.

newCsvPath (string): Path where the modified CSV file will be saved.


### Modify-TimeFormat
Modifies time format in the CSV file by replacing a specific time (e.g., 5:06:00) with a new time (e.g., 3:56).
#### Parameters:
localFilePath (string): Path to the input CSV file.

newCsvPath (string): Path where the modified CSV file will be saved.


### Verify-OutputFile
Verifies if a processed CSV file exists in the specified S3 output bucket. It will keep checking every 5 minutes until the file is found.
#### Parameters:
fileName (string): The original CSV file name.

timeStamp (string): The timestamp used in the processed file name.

fileExtension (string): The file extension (e.g., .csv).

s3OutputBucket (string): The S3 output bucket name.

awsProfile (string, required): The AWS profile to use. Scripts pass this from `config`.


### Verify-OutputLimited
Verifies if a processed CSV file exists in the S3 output bucket after waiting for 10 minutes.
#### Parameters:
fileName (string): The original CSV file name.

timeStamp (string): The timestamp used in the processed file name.

fileExtension (string): The file extension (e.g., .csv).

s3OutputBucket (string): The S3 output bucket name.

awsProfile (string, required): The AWS profile to use. Scripts pass this from `config`.


### Download-CsvFromS3
Downloads a processed CSV file from an S3 bucket to a local directory.
#### Parameters:
s3ProcessedFile (string): The name of the processed file in S3.

s3OutputBucket (string): The name of the output S3 bucket.

downloadPath (string): The local path where the file will be downloaded.

awsProfile (string, required): The AWS profile to use for authentication. Scripts pass this from `config`.


### Verify-LinkId
Verifies if the "doh" column is populated in the output CSV file and ensures that all records have a valid LinkID.

#### Parameters:
outputFile (string): The path to the output CSV file.

### DuplicateSourceID
Checks for duplicate SourceID entries in the CSV file and removes or reports them as necessary.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### InvalidBirthYear
Validates the birth year in the CSV file and removes or flags invalid entries.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### GenderWithSplCharacter
Geender field containing special characters.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### LeadingTrailingSpaceGender
Removes leading and trailing spaces in gender fields in the CSV file.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### SSNWithSplCharacter
Social Security Number (SSN) fields that contain special characters.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### LeadingTrailingSpaceSSN
Removes leading and trailing spaces in SSN fields in the CSV file.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### Remove-MandatoryFieldsFromCsv
Removes mandatory fields from the CSV file based on specific criteria.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### ExpandLimit
Expands the limit of source id in the csv input file.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### Remove-OptionalFieldsFromCSV
Removes optional fields from the CSV file that are not necessary for processing.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.

### IncorrectDateTimeofSource
Fixes incorrect date and time formats or values in the CSV file.  
#### Parameters:
- **localFilePath** (string): Path to the input CSV file.
- **newCsvPath** (string): Path where the modified CSV file will be saved.


## Requirements
PowerShell 5.1+ (Windows) or PowerShell Core (macOS/Linux).

AWS CLI installed and configured with valid credentials.

## Installation
Clone or download the repository containing the psm1 file.

Import the module into your PowerShell session
