using Microsoft.AspNetCore.Http;

namespace HCA.Infrastructure.Http;

public class VeratoTenantContext(IHttpContextAccessor httpContextAccessor) : IVeratoTenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public bool UseNonCoalitionTenant
        => string.Equals(
            _httpContextAccessor.HttpContext?.Items["TenantDatabase"]?.ToString(),
            "NonCoalition",
            StringComparison.OrdinalIgnoreCase);
}
