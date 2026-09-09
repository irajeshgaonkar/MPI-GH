//using Amazon.S3;

//namespace HCA.Infrastructure.FileReadWriter;

//public interface IFileWriter
//{
//    Task WriteFileToBucket(string bucketName, string fileName, MemoryStream stream);
//}

//public class FileWriter : IFileWriter
//{
//    private readonly IAmazonS3 _amazonS3Client;

//    public FileWriter(IAmazonS3 amazonS3)
//    {
//        _amazonS3Client = amazonS3;
//    }

//    public async Task WriteFileToBucket(string bucketName, string fileName, MemoryStream stream)
//    {
//        await _amazonS3Client.UploadObjectFromStreamAsync(bucketName, fileName, stream, new Dictionary<string, object>());
//    }
//}

