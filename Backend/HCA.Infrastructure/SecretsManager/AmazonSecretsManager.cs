using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using HCA.Infrastructure.Configurations;
using System.Text.Json;

namespace HCA.Infrastructure.SecretsManager
{
    public static class AmazonSecretsManager
    {
        public static AppSettings GetAppSettings()
        {
            var appSettings = AmazonSecretsManager.GetSecret("AppSettings").Result;
            return JsonSerializer.Deserialize<AppSettings>(appSettings)
                                      ?? throw new ArgumentException("Failed to load AppSettings from AWS SecretManager");
        }

        /// <summary>
        /// Retrieve a secret from AWS Secret Manager based on secret name.
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns>Secret value as a string.</returns>
        // TODO: wrapper that returns the json object? Possibly as individual classes in infrastructure
        public static async Task<string> GetSecret( string secretName )
        {
            string region = "us-west-2";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync( request );
            }
            catch( Exception )
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw;
            }

            return response.SecretString;
        }
    }
}
