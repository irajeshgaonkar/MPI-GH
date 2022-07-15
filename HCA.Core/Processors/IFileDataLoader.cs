namespace HCA.Core.Processors;

public interface IFileDataLoader
{
    Task ProcessFile(string fileName, StreamReader stream);
}

