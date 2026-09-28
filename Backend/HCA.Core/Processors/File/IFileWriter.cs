using HCA.Data.Entities;

namespace HCA.Core.Processors.File;


public interface IFileWriter
{
    Task<MemoryStream> WriteFile(FileRequestEntity fileRequestEntity);
}