import sys
from awsglue.transforms import *
from awsglue.utils import getResolvedOptions
from pyspark.context import SparkContext
from awsglue.context import GlueContext
from awsglue.job import Job

args = getResolvedOptions(sys.argv, ["JOB_NAME", 'bucket', 'request_id'])
sc = SparkContext()
glueContext = GlueContext(sc)
spark = glueContext.spark_session
job = Job(glueContext)
job.init(args["JOB_NAME"] + args['request_id'], args)

bucket = args['bucket']
request_id = args['request_id']
file_name = f'data/data_{request_id}.csv'

print(f'Bucket: {bucket} | File Name: {file_name}')

# Script generated for node S3 bucket
S3bucket_node1 = glueContext.create_dynamic_frame.from_options(
    format_options={
        "quoteChar": '"',
        "withHeader": True,
        "separator": ",",
        "optimizePerformance": False,
    },
    connection_type="s3",
    format="csv",
    connection_options={
        "paths": [
            f's3://{bucket}/{file_name}'
        ],
        "recurse": True,
    },
    transformation_ctx="S3bucket_node1",
)

# Script generated for node ApplyMapping
ApplyMapping_node2 = ApplyMapping.apply(
    frame=S3bucket_node1,
    mappings=[
        ("batch_number", "string", "batch_number", "int"),
        ("status", "string", "status", "string"),
        ("message", "string", "message", "string"),
        ("retry_count", "string", "retry_count", "int"),
        ("request_id", "string", "request_id", "string"),
        ("tracking_id", "string", "tracking_id", "string"),
        ("source_system_name", "string", "source_system_name", "string"),
        ("source_system_agency", "string", "source_system_agency", "string"),
        ("mpi_link_id", "string", "mpi_link_id", "string"),
        ("source_system_id", "string", "source_system_id", "string"),
        ("source_system_updated", "string", "source_system_updated", "string"),
        ("first_name", "string", "first_name", "string"),
        ("middle_name", "string", "middle_name", "string"),
        ("last_name", "string", "last_name", "string"),
        ("name_suffix", "string", "name_suffix", "string"),
        ("dob", "string", "dob", "string"),
        ("gender", "string", "gender", "string"),
        ("SSN", "string", "SSN", "string"),
        ("address_type", "string", "address_type", "string"),
        ("address_line_1", "string", "address_line_1", "string"),
        ("address_line_2", "string", "address_line_2", "string"),
        ("address_line_3", "string", "address_line_3", "string"),
        ("city", "string", "city", "string"),
        ("state", "string", "state", "string"),
        ("zip_code", "string", "zip_code", "string"),
        ("zip_four", "string", "zip_four", "string"),
        ("phone_type", "string", "phone_type", "string"),
        ("phone_number", "string", "phone_number", "string"),
        ("email_type", "string", "email_type", "string"),
        ("email_address", "string", "email_address", "string"),
        ("protected_population_flag", "string", "protected_population_flag", "string"),
        ("protected_population_type", "string", "protected_population_type", "string"),
        ("custom_json", "string", "custom_json", "string"),
    ],
    transformation_ctx="ApplyMapping_node2",
)

# Script generated for node PostgreSQL table
PostgreSQLtable_node3 = glueContext.write_dynamic_frame.from_catalog(
    frame=ApplyMapping_node2,
#Environment specific values, replace during deployment
    database="${MPI_DATABASE}",
    table_name="${MPI_FILE_REQUESTS_TABLE}",
    
    transformation_ctx="PostgreSQLtable_node3",
)

job.commit()
