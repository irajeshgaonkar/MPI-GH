using Amazon.S3;
using HCA.Infrastructure.Logger;

namespace HCA.Infrastructure.S3
{
    public class HcaS3Client : IHcaS3Client
    {
        private readonly IAppLogger _appLogger;

        private readonly IAmazonS3 _s3Client;

        public HcaS3Client(IAppLogger appLogger)
        {
            _appLogger = appLogger;
            _s3Client = new AmazonS3Client();
        }

        public async Task<MemoryStream> DownloadFileAsync(string bucketName, string fileName)
        {
            try
            {
                var memoryStream = await DownloadFile(bucketName, fileName);
                _appLogger.LogInformation($"Successfully downloaded file to s3 bucket:{bucketName} fileName:{fileName}");
                return memoryStream;
            }
            catch (Exception e)
            {
                _appLogger.LogError(e, $"Error downloading the file to S3 bucket:{bucketName} fileName:{fileName} ");
                throw;
            }
        }

        public async Task UploadFileAsync(MemoryStream stream, string bucketName, string fileName)
        {
            try
            {
                await _s3Client.UploadObjectFromStreamAsync(bucketName, fileName, stream, new Dictionary<string, object>());
                _appLogger.LogInformation($"Successfully uploaded file to s3 bucket:{bucketName} fileName:{fileName}");
            }
            catch (Exception e)
            {
                _appLogger.LogError(e, $"Error uploading the file to S3 bucket:{bucketName} fileName:{fileName} ");
                throw;
            }
        }

        private async Task<MemoryStream> DownloadFile(string bucketName, string fileName)
        {
            var fileContent = await _s3Client.GetObjectAsync(bucketName, fileName);
            var memoryStream = new MemoryStream();

            using (Stream responseStream = fileContent.ResponseStream)
            {
                responseStream.CopyTo(memoryStream);
            }
            // Set the position to the beginning of the stream.
            memoryStream.Seek(0, SeekOrigin.Begin);
            _appLogger.LogInformation($"Successfully download file to s3 bucket - Stream length:{memoryStream.Length}");

            return memoryStream;
        }
    }
}
