namespace HCA.Core.Processors;

public interface IFileDataLoader
{
    Task<Guid> ProcessFile(string fileName, StreamReader stream);
}

