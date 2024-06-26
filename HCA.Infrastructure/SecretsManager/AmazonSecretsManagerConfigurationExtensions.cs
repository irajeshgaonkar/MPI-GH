using Microsoft.Extensions.Configuration;

namespace HCA.Infrastructure.SecretsManager
{
    public static class AmazonSecretsManagerConfigurationExtensions
    {
        /// <summary>
        /// Adds Amazon Secrets Manager to <paramref name="configurationBuilder"/>, using <paramref name="region"/>.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="region">AWS region to load from.</param>
        /// <param name="secretName">Name of secret to load.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAmazonSecretsManager( this IConfigurationBuilder configurationBuilder,
            string region,
            string secretName )
        {
            AmazonSecretsManagerConfigurationSource configurationSource =
            new AmazonSecretsManagerConfigurationSource(region, secretName);

            return configurationBuilder.Add( configurationSource as IConfigurationSource );
        }

        /// <summary>
        /// Adds Amazon Secrets Manager to <paramref name="configurationBuilder"/>, defaulting to us-west-2 region.
        /// </summary>
        /// <param name="secretName">Name of secret to load.</param>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddAmazonSecretsManager( this IConfigurationBuilder configurationBuilder,
    string secretName ) => AddAmazonSecretsManager( configurationBuilder, "us-west-2", secretName );
    }
}
