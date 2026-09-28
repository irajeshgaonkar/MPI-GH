using System.Data.Common;
using System.Diagnostics;
using System.Collections.Concurrent;
using Amazon.Lambda.Core;
using HCA.Data;
using HCA.Infrastructure;
using HCA.Infrastructure.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HCA.MPI.ReportRefresher.Lambda;

public class Function
{
    private static readonly string[] MaterializedViews =
    [
        "coalitionmpi.mv_report_source_system_quality",
        "coalitionmpi.mv_report_source_system_protected_population",
        "coalitionmpi.mv_report_mpi_link_clusters",
        "coalitionmpi.mv_report_source_fragmentation",
        "coalitionmpi.mv_report_stewardship_request_events",
        "coalitionmpi.mv_report_batch_file_summary",
        "coalitionmpi.mv_report_batch_error_events",
        "coalitionmpi.mv_report_manual_stewardship_queue",
        "coalitionmpi.mv_report_system_reference",
        "coalitionmpi.mv_report_data_sharing_coverage",
        "coalitionmpi.mv_report_link_id_ingest_trend"
    ];

    private readonly IServiceProvider _serviceProvider;

    public Function()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddAppSettings(configuration);
        services.AddConsoleLogging();
        services.AddDbContext(configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task<MaterializedViewRefreshResponse> FunctionHandler(
        MaterializedViewRefreshRequest? input,
        ILambdaContext context)
    {
        var logger = new AppLogger(context.Logger);
        var useConcurrentRefresh = input?.UseConcurrentRefresh ?? true;
        var maxDegreeOfParallelism = Math.Clamp(
            input?.MaxDegreeOfParallelism ?? 4,
            1,
            MaterializedViews.Length);
        var startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var refreshedViews = new ConcurrentBag<string>();
        logger.LogInformation(
            $"Starting materialized view refresh. RequestId={context.AwsRequestId}, ConcurrentRefresh={useConcurrentRefresh}, MaxDegreeOfParallelism={maxDegreeOfParallelism}, ViewCount={MaterializedViews.Length}, StartedAtUtc={startedAtUtc:O}");

        try
        {
            await Parallel.ForEachAsync(
                MaterializedViews,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism
                },
                async (viewName, cancellationToken) =>
                {
                    await RefreshViewAsync(
                        viewName,
                        useConcurrentRefresh,
                        refreshedViews,
                        logger,
                        cancellationToken);
                });

            stopwatch.Stop();
            var completedAtUtc = DateTime.UtcNow;
            logger.LogInformation(
                $"Completed materialized view refresh successfully. RequestId={context.AwsRequestId}, CompletedAtUtc={completedAtUtc:O}, DurationMs={stopwatch.ElapsedMilliseconds}.");

            return new MaterializedViewRefreshResponse
            {
                Success = true,
                UsedConcurrentRefresh = useConcurrentRefresh,
                StartedAtUtc = startedAtUtc,
                CompletedAtUtc = completedAtUtc,
                DurationMilliseconds = stopwatch.ElapsedMilliseconds,
                RefreshedViews = MaterializedViews
                    .Where(viewName => refreshedViews.Contains(viewName))
                    .ToArray()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex,
                $"Materialized view refresh failed. RequestId={context.AwsRequestId}, StartedAtUtc={startedAtUtc:O}, DurationMs={stopwatch.ElapsedMilliseconds}");
            throw;
        }
    }

    private static async Task EnsureOpenAsync(DbConnection connection)
    {
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
    }

    private async Task RefreshViewAsync(
        string viewName,
        bool useConcurrentRefresh,
        ConcurrentBag<string> refreshedViews,
        IAppLogger logger,
        CancellationToken cancellationToken)
    {
        var viewStopwatch = Stopwatch.StartNew();
        logger.LogInformation($"Refreshing materialized view {viewName}.");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HcaDbContext>();
        dbContext.Database.SetCommandTimeout(0);

        await using var connection = dbContext.Database.GetDbConnection();
        await EnsureOpenAsync(connection);

        await using var command = connection.CreateCommand();
        command.CommandText = $"{(useConcurrentRefresh ? "REFRESH MATERIALIZED VIEW CONCURRENTLY" : "REFRESH MATERIALIZED VIEW")} {viewName};";
        command.CommandType = System.Data.CommandType.Text;
        command.CommandTimeout = 0;
        await command.ExecuteNonQueryAsync(cancellationToken);

        viewStopwatch.Stop();
        refreshedViews.Add(viewName);
        logger.LogInformation(
            $"Completed refresh for {viewName} in {viewStopwatch.ElapsedMilliseconds} ms.");
    }
}
