namespace HCA.MPI.ReportRefresher.Lambda;

public class MaterializedViewRefreshRequest
{
    public bool UseConcurrentRefresh { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = 4;
}
