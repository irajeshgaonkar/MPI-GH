namespace HCA.Models.Response
{
    public class IdentityLinkIdResponse : BaseResponse
    {
        public IdentityLinkIdContent Content { get; set; } = new();
    }

    public class IdentityLinkIdContent
    {
        public string? LinkId { get; set; }
    }
}
