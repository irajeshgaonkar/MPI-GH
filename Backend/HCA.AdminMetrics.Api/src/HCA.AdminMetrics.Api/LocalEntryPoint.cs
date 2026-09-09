namespace HCA.AdminMetrics.Api;

/// <summary>
/// Provides the local application entry point for development hosting.
/// </summary>
public class LocalEntryPoint
{
    /// <summary>
    /// Starts the Admin Metrics API using the default host builder.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    /// <summary>
    /// Creates the host builder used to run the Admin Metrics API locally.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The configured host builder.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
