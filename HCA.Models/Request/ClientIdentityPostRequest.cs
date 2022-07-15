namespace HCA.Models.Request
{
    public class ClientIdentityPostRequest : BaseRequest
    {
        public ClientIdentityPostRequest(Guid requestId, string trackingId)
            : base(requestId, trackingId)
        { }
    }
}

