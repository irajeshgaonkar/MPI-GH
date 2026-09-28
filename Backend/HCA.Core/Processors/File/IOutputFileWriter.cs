namespace HCA.Core.Processors.File;

public interface IOutputFileWriter
{
    Task WriteFile(string requestId);
}
