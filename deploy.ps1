# deploy.ps1
<#

Foolproof steps!
1. Verify branch, code, access tokens in appsettings.json
2. Check aws CLI access (necessary for upload step)
3. run .\deploy.ps1 [dev|test|prod]
4. The script Builds, zips, deploys
5. If your aws CLI isn't set up correctly, can manually go to [codeDirectory]mpi_api\Release - .zips will be there and can be manually uploaded

Note: Your User\.aws\credentials file must have a functioning profile for the target environment:
[MPI-Dev]
[MPI-Integration]
[MPI-Test]
[MPI-Prod]

Ensure that the profiles target the correct environments. Even without profiles, this script will still publish and zip for you to \Release, which is useful.
The script DOES NOT update appsettings.json (yet).
#>

[CmdletBinding()]
Param(
    # Environment that the lambda will be updated to: Dev, Test or Prod
    [Parameter(Mandatory, Position = 0, HelpMessage = "Enter environment: Dev|Integration|Test|Prod")]
    [ValidateSet("Dev", "Integration","Int", "Test", "Prod")]
    [String]$Environment
)

# list of VS projects and corresponding lambda names
$projectsToLambdas = @{
    "HCA.Api"                        = "mpi-frontend-api-lambda" ;
    "HCA.Batch.SQS.Publisher.Lambda" = "mpi-batch-processing-sqs";
    "HCA.MuleSoft.Lambda"            = "mpi-mulesoft-api-lambda";
    "HCA.Sftp.Lambda"                = "mpi-sftp";
}

# TODO: more thorough testing before using in prod
$environmentToProfile = @{
    "Dev"  = "MPI-Dev"
    "Integration" = "MPI-Integration"
    "Int" = "MPI-Integration"
    "Test" = "MPI-Test"
    "Prod" = "TODO:MPI-Prod"
}

# TODO: Pull in appsettings.json dynamically

dotnet publish -f net6.0 -c Release

try { Get-Command aws > $null }
catch {
    Write-Warning "You need aws-cli to deploy this lambda. Google 'aws-cli install'"
    exit 1
}

$region = aws configure get region
Write-Output "Deploying MPI AWS Lambdas to ${region}:$Environment"

foreach ($project in $projectsToLambdas.Keys) {
    $lambda = $projectsToLambdas[$project]
    $zipName = "Release\$lambda.zip"
    if (Test-Path $zipName) {
        Write-Verbose "removing old $lambda zip"
        Remove-Item $zipName -verbose
    }

    $publishFolder = "$project\src\$project\bin\Release\net6.0\publish"
    # todo make parallel (see experimental branch)
    Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipName
    Write-Verbose "Zipped $project to $zipName"
}

# TODO check aws creds and run HCA_AWS_

foreach ($project in $projectsToLambdas.Keys) {
    $lambda = $projectsToLambdas[$project]
    Write-Verbose "Uploading $lambda to ${region}:$Environment"
    $awsProfile = $environmentToProfile[$Environment]
    $zipName = "Release\$lambda.zip"
    aws lambda update-function-code --function-name "$lambda" --zip-file fileb://$zipName --publish --profile $awsProfile >".\Release\upload_$lambda.log"
    if ( $?) {
        Write-Output "!! $lambda Upload successful to $Environment !!"    
    }
    else {
        Write-Output "Upload failed"
        exit 1
    }   
}