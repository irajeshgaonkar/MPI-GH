using Amazon.Lambda.AspNetCoreServer;

namespace HCA.AdminMetrics.Api;

/// <summary>
/// Provides the AWS Lambda entry point for API Gateway-hosted execution.
/// </summary>
public class LambdaEntryPoint : APIGatewayProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        builder.UseStartup<Startup>();
    }
}
