namespace HCA.Infrastructure.Http;

public interface IVeratoHttpClientFactory
{
    HttpClient Create(bool useNonCoalitionTenant, bool isEnrich);
}
