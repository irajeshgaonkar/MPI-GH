using System;
namespace HCA.MuleSoft.Lambda.Models
{
    public class RequestModel
    {
        public string OperationType { get; set; }

        public string BucketName { get; set; }

        public string FileName { get; set; }

        public string RequestId { get; set; }
    }
}

