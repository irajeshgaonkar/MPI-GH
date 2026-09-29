# Glue job for reading the file and loading the database 

import sys
import io
import logging
import boto3
from botocore.exceptions import ClientError
from awsglue.utils import getResolvedOptions
import pandas as pd
from datetime import datetime

def download_file_from_bucket(bucket_name, s3_key, dst_path):
    print(f'started downloading the file - {bucket_name}:{s3_key}')
    s3_resource = boto3.resource("s3")
    bucket = s3_resource.Bucket(bucket_name)
    bucket.download_file(Key=s3_key, Filename=dst_path)
    print(f'completed downloading the file - {bucket_name}:{s3_key}')

def upload_data_to_bucket(bucket_name, s3_key, file_name):
    print(f'started uploading the file - {bucket_name}:{s3_key}')
    s3_resource = boto3.resource("s3")
    obj = s3_resource.Object(bucket_name, s3_key)
    obj.upload_file(Filename=file_name)
    print(f'completed uploading the file - {bucket_name}:{s3_key}')
    
def get_meta_field(meta_fields, index):
    if len(meta_fields) <= index:
        return None
    
    meta_field = meta_fields[index]
    meta_field = meta_field.split('(')[0].strip()
    return meta_field
    
def is_non_empty_line(fields):
    return len(fields) > 1 and fields[1].strip()
    
def is_trailer_line(fields):
    return len(fields) > 1 and fields[0].strip() == 'TRALR'
    
job_parameters = (getResolvedOptions(sys.argv,['source_bucket','intermediate_bucket', 'source_file', 'request_id']))

# #Get the parameters  - Start
source_bucket = job_parameters['source_bucket']
intermediate_bucket = job_parameters['intermediate_bucket']
source_file = job_parameters['source_file']
request_id = job_parameters['request_id']

data_file = f'data_{request_id}.csv'
meta_data_file = f'meta_{request_id}.csv'
batch_size = 50

print (f"request_id: {request_id} | First Bucket  Name:{source_bucket} | Input File  Name: {source_file} | Output Bucket Name: {intermediate_bucket} | data File Name: {data_file} | meta data File Name: {meta_data_file}")

print('Started downloading the file')
#download the file from S3
source_file_in_memory = source_file.replace(".csv", "_in_memory.csv")
data_file_inMemory = data_file.replace(".csv", "_in_memory.csv")
meta_data_file_inMemory = meta_data_file.replace(".csv", "_in_memory.csv")
download_file_from_bucket(source_bucket, f'{source_file}', source_file_in_memory)
print('Completed downloading the file')

# df = pd.read_csv(source_file_in_memory)
# print(df)

source_system_name = ''
source_agency = ''
api_call_type = ''
trailer = ''
file_create_date_time = ''
tracking_id = ''
status = 'Not Started'
tracking_id = request_id

with open(source_file_in_memory, 'r') as input_file, open(data_file_inMemory, 'w') as output_file:
    line_number = 1;
    records_count = 0;

    data_header = 'batch_number,status,message,retry_count,request_id,tracking_id,source_system_name,source_system_agency,mpi_link_id,source_system_id,source_system_updated,first_name,middle_name,last_name,name_suffix,dob,gender,SSN,address_type,address_line_1,address_line_2,address_line_3,city,state,zip_code,zip_four,phone_type,phone_number,email_type,email_address,protected_population_flag,protected_population_type,custom_json\n'
    output_file.write(data_header)
    
    for line in input_file:
        batch_number = (records_count // batch_size) + 1
        

        if line_number == 1:
            meta_data_fields = line.split(',')
            source_system_name = get_meta_field(meta_data_fields, 1)
            source_system_agency = get_meta_field(meta_data_fields, 0)
            api_call_type = get_meta_field(meta_data_fields, 4)
            file_created_date = get_meta_field(meta_data_fields, 2)
            file_created_time = get_meta_field(meta_data_fields, 3)
            #tracking_id = get_meta_field(meta_data_fields, 5)
            datetime_object = datetime.now()
            if file_created_time:               
                file_create_date_time_str = f'{file_created_date} {file_created_time}'
                datetime_object = datetime.strptime(file_create_date_time_str, '%m/%d/%Y %H:%M:%S')
            else:
                file_create_date_time_str = f'{file_created_date}'
                datetime_object = datetime.strptime(file_create_date_time_str, '%m/%d/%Y')
                
            file_create_date_time = datetime_object.strftime("%Y-%m-%d %H:%M:%S")
            
            
        elif  line != '\n' and line_number >= 2:
            fields = line.split(',')
            custom_json = ''
            
            if is_trailer_line(fields):
                trailer = fields[1]
                
            elif is_non_empty_line(fields):
                
                if (len(fields) > 24):
                    custom_json = '"{';
                    
                    for i in range(24, len(fields)):
                        custom_json += f'{i - 24}:' + "'" + f'{fields[i].strip()}' + "'"
                        
                        if (i != len(fields) - 1):
                            custom_json += ','
                    
                    custom_json += '}"'
                
                line = f'{batch_number},{status}, ,0,{request_id}, ,{source_system_name},{source_system_agency},{fields[0]},{fields[1]},{fields[2]},{fields[3]},{fields[4]},{fields[5]},{fields[6]},{fields[7]},{fields[8]},{fields[9]},{fields[10]},{fields[11]},{fields[12]},{fields[13]},{fields[14]},{fields[15]},{fields[16]},{fields[17]},{fields[18]},{fields[19]},{fields[20]},{fields[21]},{fields[22]},{fields[23].strip()},{custom_json}\r\n'
                output_file.write(line)
                records_count += 1
        
        line_number += 1

# api_call_type = 'VE Post'
message = ''

with open(meta_data_file_inMemory, 'w') as meta_data_output_file:
    output_file_name = source_file.replace('.csv', '_processed.csv')
    #ouput_file_name = source_file
    meta_data_output_file.write('file_name,output_file_name,source_system_agency,source_system_name,records_count,trailer,api_type,file_created_date_time,tracking_id,request_id,request_date_time,status,message\n')
    meta_data_output_file.write(f'{source_file},{output_file_name},{source_system_agency},{source_system_name},{records_count},{trailer},{api_call_type},{file_create_date_time},{tracking_id},{request_id},{datetime.now().strftime("%Y-%m-%d %H:%M:%S")},{status},{message}')
    
upload_data_to_bucket(intermediate_bucket, f'data/{data_file}', data_file_inMemory)
upload_data_to_bucket(intermediate_bucket, f'meta-data/{meta_data_file}', meta_data_file_inMemory)
