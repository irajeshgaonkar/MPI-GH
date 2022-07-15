using HCA.Core.Processors.CsvFileProcessor;
using HCA.Models.Request;

namespace HCA.Core.Processors;

public interface IFileParser
{
    (IEnumerable<ClientIdentityRequest>, Dictionary<int, string>, FileHeaderDataModel) ParseFile(StreamReader stream);
}

