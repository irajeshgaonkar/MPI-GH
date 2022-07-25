using HCA.Data.Entities;
using HCA.FileProcessor.Models;
using HCA.Models.Request;

namespace HCA.Core.Mapper;

/// <inheritdoc/>
public interface IClientIdentityRequestMapper : IMapper<Data.Entities.ClientIdentityRequestEntity, Models.Request.ClientIdentityRequest>
{

}

public interface IFileClientIdentityMapper : IMapper<Data.Entities.ClientIdentityRequestEntity, FileClientIdentity>
{

}

