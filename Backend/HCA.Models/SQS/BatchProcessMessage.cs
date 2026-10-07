using HCA.Models.Request;
namespace HCA.Models.SQS;

public class BatchProcessMessage
{
    public string ApiCallType { get; set; }

    public string TenantDatabase { get; set; } = "HHS Coalition";

    public IEnumerable<ClientIdentityRequest> ClientIdentityRequests { get; set; }
}

public class OuputFileGenerationMessage
{
    public string RequestId { get; set; }

    public string TenantDatabase { get; set; } = "HHS Coalition";
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
