# deploy.ps1

# TODO: param for switching deployment target?

# Param([string]$arg1)
# if(!$arg1){
#     $scriptName = Split-Path -leaf $PSCommandpath
#     Write-Warning "Usage : $scriptName lambdaName"
#     exit 1
# }

# list of VS projects and corresponding lambda names
$projectsToLambdas = @{
    "HCA.Api"                        = "mpi-frontend-api-lambda" ;
    "HCA.Batch.SQS.Publisher.Lambda" = "mpi-batch-processing-sqs";
    "HCA.MuleSoft.Lambda"            = "mpi-mulesoft-api-lambda";
    "HCA.Sftp.Lambda"                = "mpi-sftp";
}

dotnet publish -f net6.0 -c Release

# TODO: put aws check back in once uploading is supported
# try { Get-Command aws > $null }
# catch {
#     Write-Warning "You need aws-cli to deploy this lambda. Google 'aws-cli install'"
#     exit 1
# }

# $region = aws configure get region
# Write-Output "Deploying MPI AWS Lambdas to $region"

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

# todo: upload
# At this time, cannot upload to correct environment - coordinating with Taylor Church
# foreach ($project in $projectsToLambdas.Keys) {
#     $lambda = $projectsToLambdas[$project]
#     Write-Verbose "Uploading $lambda to $region"
#     # aws lambda update-function-code --function-name "$lambda" --zip-file fileb://$zipName --publish >".\upload_$lambda.log"
#     if ( $?) {
#         Write-Output "!! $lambda Upload successful !!"    
#     }
#     else {
#         Write-Output "Upload failed"
#         Write-Output "If the error was a 400, check that there are no slashes in your lambda name"
#         Write-Output "Lambda name = $lambda"
#         exit 1
#     }   
# }