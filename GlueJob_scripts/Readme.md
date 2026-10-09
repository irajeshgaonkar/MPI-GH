# Glue ETL jobs

| Job | Script | Type |
|---|---|---|
| `mpi_file_splitter` | `mpi_file_splitter.py` | Python shell |
| `load_file_requests` | `load_file_requests.py` | Spark ETL |
| `load-client-identity-requests` | `load-client-identity-requests.py` | Spark ETL |

The Step Functions definition calls these job names. Do not rename them.

Before updating a load job, set the catalog values in that script:

| Placeholder | `load_file_requests.py` | `load-client-identity-requests.py` |
|---|---|---|
| `${MPI_DATABASE}` | Glue Data Catalog database for MPI Postgres | same database |
| `${MPI_FILE_REQUESTS_TABLE}` | catalog table for `coalitionmpi.file_requests` | catalog table for `coalitionmpi.client_identity_requests` |

Open the job in Glue, replace the script with the file from this folder, and save.