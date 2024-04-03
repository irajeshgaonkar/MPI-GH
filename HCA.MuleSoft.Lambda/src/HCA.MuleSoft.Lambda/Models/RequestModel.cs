namespace HCA.MuleSoft.Lambda.Models
{
    public class RequestModel
    {
        public string OperationType { get; set; }

        public string BucketName { get; set; }

        public string FileName { get; set; }

        public string RequestId { get; set; }
    }

    public class ResponseModel
    {
        public string operationType { get; set; }

        public string bucketName { get; set; }

        public string fileName { get; set; }

        public string requestId { get; set; }
    }
}

