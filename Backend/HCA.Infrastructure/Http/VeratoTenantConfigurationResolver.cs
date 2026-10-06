using System.Security.Cryptography.X509Certificates;
using System.Text;
using HCA.Infrastructure.Configurations;

namespace HCA.Infrastructure.Http;

public class VeratoTenantConfigurationResolver(AppSettings appSettings, IVeratoTenantContext tenantContext)
{
    private readonly AppSettings _appSettings = appSettings;
    private readonly IVeratoTenantContext _tenantContext = tenantContext;

    public VeratoResolvedConfiguration Resolve(bool isEnrich)
        => Resolve(_appSettings, _tenantContext.UseNonCoalitionTenant, isEnrich);

    public static VeratoResolvedConfiguration Resolve(AppSettings appSettings, bool useNonCoalition, bool isEnrich)
    {
        var veratoOptions = appSettings.VeratoOptions;
        var nonCoalitionOptions = veratoOptions.NonCoalition;

        var coalitionBaseUrl = isEnrich ? veratoOptions.EnrichBaseUrl : veratoOptions.BaseUrl;
        var nonCoalitionBaseUrl = isEnrich ? nonCoalitionOptions?.EnrichBaseUrl : nonCoalitionOptions?.BaseUrl;
        var baseUrl = useNonCoalition && !string.IsNullOrWhiteSpace(nonCoalitionBaseUrl)
            ? nonCoalitionBaseUrl
            : coalitionBaseUrl;

        var username = useNonCoalition
            ? isEnrich
                ? nonCoalitionOptions?.EnrichUsername
                : nonCoalitionOptions?.Username
            : isEnrich
                ? veratoOptions.EnrichUsername
                : veratoOptions.Username;

        var password = useNonCoalition
            ? isEnrich
                ? nonCoalitionOptions?.EnrichPassword
                : nonCoalitionOptions?.Password
            : isEnrich
                ? veratoOptions.EnrichPassword
                : veratoOptions.Password;

        var clientCert = useNonCoalition
            ? nonCoalitionOptions?.ClientCert
            : veratoOptions.ClientCert;

        var clientCertPassword = useNonCoalition
            ? nonCoalitionOptions?.ClientCertPassword
            : veratoOptions.ClientCertPassword;

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Verato base URL is missing.");

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException($"Verato {(useNonCoalition ? "non-coalition" : "coalition")} credentials are missing.");

        if (string.IsNullOrWhiteSpace(clientCert))
            throw new InvalidOperationException($"Verato {(useNonCoalition ? "non-coalition" : "coalition")} client certificate is missing.");

        return new VeratoResolvedConfiguration
        {
            BaseUrl = baseUrl,
            AuthorizationHeader = $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"))}",
            Certificate = new X509Certificate2(
                Convert.FromBase64String(clientCert),
                clientCertPassword,
                X509KeyStorageFlags.MachineKeySet),
            Timeout = TimeSpan.FromSeconds(veratoOptions.RequestTimeoutInSec)
        };
    }
}
