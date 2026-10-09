namespace HCA.MPI.ReportRefresher.Lambda;

public class MaterializedViewRefreshResponse
{
    public bool Success { get; set; }
    public bool UsedConcurrentRefresh { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    public long DurationMilliseconds { get; set; }
    public IReadOnlyList<string> RefreshedViews { get; set; } = Array.Empty<string>();
}
