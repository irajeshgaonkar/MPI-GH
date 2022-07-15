namespace HCA.Models.Request
{
    public abstract class BaseRequest
    {
        protected BaseRequest(Guid requestId, string trackingId)
        {
            RequestId = requestId;
            TrackingId = trackingId;
        }

        public Guid RequestId { get; set; }

        public string TrackingId { get; set; }

        public List<ClientIdentityRequest> ClientIdentity { get; set; }
    }
}

