namespace HCA.Models.Verato.Response;

public class CreateDataSourceResponse : VeratoResponse
{
    public CreateDataSourceResponseContent Content { get; set; } = new();
}

public class CreateDataSourceResponseContent
{
    public List<DataSourceCreationResponse> DatasourceCreationResponses { get; set; } = new();
}

public class DataSourceCreationResponse
{
    public string Datasource { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
