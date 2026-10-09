namespace HCA.AdminMetrics.Api.Models;

/// <summary>
/// Represents the result of refreshing the administration materialized views.
/// </summary>
public class AdminMaterializedViewRefreshResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the refresh completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether concurrent refresh mode was used.
    /// </summary>
    public bool UsedConcurrentRefresh { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the refresh started.
    /// </summary>
    public DateTime StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the refresh completed.
    /// </summary>
    public DateTime CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the total refresh duration in milliseconds.
    /// </summary>
    public long DurationMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets the list of materialized views that were refreshed.
    /// </summary>
    public List<string> RefreshedViews { get; set; } = [];
}
