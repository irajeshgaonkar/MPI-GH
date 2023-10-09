# deploy.ps1
# Param([string]$arg1)
# if(!$arg1){
#     $scriptName = Split-Path -leaf $PSCommandpath
#     Write-Warning "Usage : $scriptName lambdaName"
#     exit 1
# }

$projectsToLambdas = @{
    "HCA.Api"                        = "mpi-frontend-api-lambda" ;
    "HCA.Batch.SQS.Publisher.Lambda" = "mpi-batch-processing-sqs";
    "HCA.MuleSoft.Lambda"            = "mpi-mulesoft-api-lambda";
    "HCA.Sftp.Lambda"                = "mpi-sftp";
}

# $lambda = $arg1.Trim().Trim('/\.')
# Write-Debug "Lambda is: '$lambda'"
# if ( !(Test-Path "$lambda\") ) {
#     Write-Warning "Folder $lambda does not exist"
#     exit 1
# }
# if ( !(Test-Path "$lambda\*.py") ) {
#     Write-Warning "There are no python files in folder $lambda"
#     exit 1
# }

$region = aws configure get region
Write-Output "Deploying MPI AWS Lambdas to $region"

try { Get-Command aws > $null }
catch {
    Write-Warning "You need aws-cli to deploy this lambda. Google 'aws-cli install'"
    exit 1
}

foreach ($project in $projectsToLambdas.Keys) {
    $lambda = $projectsToLambdas[$project]
    $zipName = "Release\$lambda.zip"
    if (Test-Path $zipName) {
        Write-Verbose "removing old $lambda zip"
        Remove-Item $zipName -verbose
    }
    msbuild "$project\src\$project\$project.csproj" -p:DeployOnBuild=true -p:PublishProfile="$project\src\$project\Properties\PublishProfiles\release.pubxml"
}

# $zipName = "$lambda\archive.zip";
# if (Test-Path $zipName) {
#     Write-Verbose "removing old zip"
#     Remove-Item $zipName -verbose
# }

# Write-Verbose "creating a new zip file"
# # Only zips python files, and no subdirectories
# Compress-Archive -Path "$lambda\*.py" -DestinationPath $zipName


# Write-Verbose "Uploading $lambda to $region"

# aws lambda update-function-code --function-name "$lambda" --zip-file fileb://$zipName --publish >.\upload.log
# if ( $?) {

#     Write-Output "!! Upload successful !!"    
# }
# else {

#     Write-Output "Upload failed"
#     Write-Output "If the error was a 400, check that there are no slashes in your lambda name"
#     Write-Output "Lambda name = $lambda"
#     exit 1
# }