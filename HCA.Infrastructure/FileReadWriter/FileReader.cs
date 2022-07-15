//using System;
//using Amazon.S3;

//namespace HCA.Infrastructure.FileReadWriter;

//public interface IFileReader
//{
//    Task<StreamReader> ReadFileFromBucket(string bucketName, string fileName);
//}

//public class FileReader : IFileReader
//{
//    private readonly IAmazonS3 _amazonS3Client;

//    public FileReader(IAmazonS3 amazonS3)
//    {
//        _amazonS3Client = amazonS3;
//    }

//    public async Task<StreamReader> ReadFileFromBucket(string bucketName, string fileName)
//    {
//        var fileContent = await _amazonS3Client.GetObjectAsync(bucketName, fileName);
//        var fileStream = fileContent.ResponseStream;
//        var streamReader = new StreamReader(fileStream);
//        return streamReader;
//    }
//}

