namespace HCA.Data;

public interface IDataAdapter
{
    Task<bool> ExecuteNonQueryAsync(string storedProcedureName, IDictionary<string, object?> parameters, int? timeout = null);
}