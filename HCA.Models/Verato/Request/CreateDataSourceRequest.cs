namespace HCA.Models.Verato.Request;

public class CreateDataSourceRequest(string trackingId) : VeratoRequest(trackingId)
{
    public CreateDataSourceRequestContent Content { get; set; } = new();
}

public class CreateDataSourceRequestContent
{
    public List<string> Sources { get; set; } = new();
}
