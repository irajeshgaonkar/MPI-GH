using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace HCA.Infrastructure.SecretsManager
{
    public class AmazonSecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly string _region;
        private readonly string _secretName;

        public AmazonSecretsManagerConfigurationProvider( string region, string secretName )
        {
            _region = region;
            _secretName = secretName;
        }

        public override void Load()
        {
            string secret = GetSecret();

            Data = JsonSerializer.Deserialize<Dictionary<string, string>>( secret );
        }

        private string GetSecret()
        {
            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = _secretName,
                VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
            };

            using( AmazonSecretsManagerClient client =
            new AmazonSecretsManagerClient( Amazon.RegionEndpoint.GetBySystemName( _region ) ) )
            {
                GetSecretValueResponse response = client.GetSecretValueAsync(request).Result;

                string secretString;
                if( response.SecretString != null )
                {
                    secretString = response.SecretString;
                }
                else
                {
                    MemoryStream memoryStream = response.SecretBinary;
                    StreamReader reader = new StreamReader(memoryStream);
                    secretString =
            System.Text.Encoding.UTF8
                .GetString( Convert.FromBase64String( reader.ReadToEnd() ) );
                }

                return secretString;
            }
        }
    }
}
