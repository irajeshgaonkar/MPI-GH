using HCA.Models.Request;
namespace HCA.Models.SQS;

public class BatchProcessMessage
{
    public string ApiCallType { get; set; }

    public IEnumerable<ClientIdentityRequest> ClientIdentityRequests { get; set; }
}

public class SqsMessage
{
    public string MessageType { get; set; }

    public string Payload { get; set; }
}

public class UserRequestMessage
{
    public string ApiCallType { get; set; }

    public UserRequest UserRequest { get; set; }

}
