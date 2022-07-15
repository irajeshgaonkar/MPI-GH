namespace HCA.Core.Processors.CsvFileProcessor;

public interface IFileReader
{
    List<string> ReadLines(StreamReader streamReader);
}

