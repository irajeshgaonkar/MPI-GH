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
file_name = f'meta-data/meta_{request_id}.csv'

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
        "paths": [f's3://{bucket}/{file_name}'],
        "recurse": False,
    },
    transformation_ctx="S3bucket_node1",
)

# Script generated for node ApplyMapping
ApplyMapping_node2 = ApplyMapping.apply(
    frame=S3bucket_node1,
    mappings=[
        ("file_name", "string", "file_name", "string"),
        ("output_file_name", "string", "output_file_name", "string"),
        ("source_system_agency", "string", "source_system_agency", "string"),
        ("source_system_name", "string", "source_system_name", "string"),
        ("records_count", "string", "records_count", "int"),
        ("trailer", "string", "trailer", "string"),
        ("api_type", "string", "api_type", "string"),
        ("file_created_date_time", "string", "file_created_date_time", "timestamp"),
        ("tracking_id", "string", "tracking_id", "string"),
        ("request_id", "string", "request_id", "string"),
        ("request_date_time", "string", "request_date_time", "timestamp"),
        ("status", "string", "status", "string"),
        ("message", "string", "message", "string"),
    ],
    transformation_ctx="ApplyMapping_node2",
)

print('completed transformation')

# Script generated for node PostgreSQL table
PostgreSQLtable_node3 = glueContext.write_dynamic_frame.from_catalog(
    frame=ApplyMapping_node2,
#Environment specific values, replace during deployment
    database="${MPI_DATABASE}",
    table_name="${MPI_FILE_REQUESTS_TABLE}",
    transformation_ctx="PostgreSQLtable_node3",
)

print('completed loading data')

job.commit()
