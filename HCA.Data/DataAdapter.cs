using System.Data;
using Npgsql;
using HCA.Infrastructure.Logger;

namespace HCA.Data;

public class DataAdapter : IDataAdapter
{
    private ILogger _logger;

    private readonly string _connectionString;

    public DataAdapter(ILogger logger, ConnectionDetails connectionDetails)
    {
        _connectionString = connectionDetails.ConnectionString;
        _logger = logger;
    }

    public async Task<bool> ExecuteNonQueryAsync(string storedProcedureName, IDictionary<string, object?> parameters, int? timeout = null)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(storedProcedureName, conn);
            cmd.CommandType = CommandType.Text;

            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
            }

            await cmd.ExecuteNonQueryAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex);
            throw;
        }
    }
}

