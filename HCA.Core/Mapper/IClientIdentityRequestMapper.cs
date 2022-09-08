using HCA.Data.Entities;
using HCA.FileProcessor.Models;
using HCA.Models.Request;

namespace HCA.Core.Mapper;

/// <inheritdoc/>
public interface IClientIdentityRequestMapper : IMapper<ClientIdentityRequestEntity, ClientIdentityRequest>
{

}

public interface IFileClientIdentityMapper : IMapper<ClientIdentityRequestEntity, FileClientIdentity>
{

}

public interface IUserRequestMapper : IMapper<UserRequestEntity, UserRequest>
{

}

public interface IFileRequestMapper : IMapper<FileRequestEntity, FileRequest>
{

}
