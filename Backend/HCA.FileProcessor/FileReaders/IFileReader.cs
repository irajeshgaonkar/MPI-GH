namespace HCA.FileProcessor.FileReaders;

public interface IFileReader
{
    List<string> ReadLines(StreamReader streamReader);
}