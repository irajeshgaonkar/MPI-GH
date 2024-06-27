using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using HCA.Infrastructure.Sftp;
using System.Text.Json;

namespace HCA.Infrastructure.SecretsManager
{
    public static class AmazonSecretsManager
    {
        public static SftpOptions GetSftpOptions()
        {
            string sftpSecret = AmazonSecretsManager.GetSecret("sftp").Result;
            return JsonSerializer.Deserialize<SftpOptions>(sftpSecret)
                                      ?? throw new ArgumentException("Sftp secret load issue.");
        }

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
