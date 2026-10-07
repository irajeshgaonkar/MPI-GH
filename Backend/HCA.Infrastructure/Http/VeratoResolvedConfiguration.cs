using System.Security.Cryptography.X509Certificates;

namespace HCA.Infrastructure.Http;

public class VeratoResolvedConfiguration
{
    public string BaseUrl { get; init; } = string.Empty;
    public string AuthorizationHeader { get; init; } = string.Empty;
    public X509Certificate2 Certificate { get; init; } = null!;
    public TimeSpan Timeout { get; init; }
}
