# MPI batch Step Functions

`mpi-step-function.json` is the state machine definition. An EventBridge rule on the source bucket starts it when a file is created. It runs `mpi_file_splitter`, `load_file_requests`, and `load-client-identity-requests`, then invokes the batch SQS publisher Lambda.

Set these values in the definition before saving it:

| Placeholder | Value |
|---|---|
| `${MPI_BATCH_PROCESSING_OUTPUT_BUCKET}` | intermediate S3 bucket |
| `${MPI_BATCH_PROCESSING_LAMBDA_ARN}` | ARN of `HCA.Batch.SQS.Publisher.Lambda` |

Remove the trailing comma after each `bucket` value in `ResultSelector`.

Open the state machine in Step Functions, edit the definition, paste this file, and save. The execution role needs permission to start the Glue jobs and invoke that Lambda.