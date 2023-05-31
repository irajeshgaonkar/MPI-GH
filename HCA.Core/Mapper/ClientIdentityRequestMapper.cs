using AutoMapper;
using HCA.Data.Entities;
using HCA.FileProcessor.Models;
using HCA.Models.Request;

namespace HCA.Core.Mapper;

/// <inheritdoc/>
public class ClientIdentityRequestMapper : IClientIdentityRequestMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// <see cref="ClientIdentityRequestMapper"/>
    /// </summary>
    /// <param name="mapper">Auto mapper</param>
    public ClientIdentityRequestMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public ClientIdentityRequest MapToModel(ClientIdentityRequestEntity entity)
    {
        return _mapper.Map<ClientIdentityRequest>(entity);
    }

    /// <inheritdoc/>
    public ClientIdentityRequestEntity MapToEntity(ClientIdentityRequest model)
    {
        return _mapper.Map<ClientIdentityRequestEntity>(model);
    }

    /// <inheritdoc/>
    public IEnumerable<ClientIdentityRequest> MapToModelCollection(IEnumerable<ClientIdentityRequestEntity> entities)
    {
        var collection = new List<ClientIdentityRequest>();

        foreach (var entity in entities)
        {
            collection.Add(MapToModel(entity));
        }

        return collection;
    }

    /// <inheritdoc/>
    public IEnumerable<ClientIdentityRequestEntity> MapToEntityCollection(IEnumerable<Models.Request.ClientIdentityRequest> models)
    {
        var entities = new List<ClientIdentityRequestEntity>();

        foreach (var model in models)
        {
            entities.Add(MapToEntity(model));
        }

        return entities;
    }
}

/// <inheritdoc/>
public class UserRequestMapper : IUserRequestMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// <see cref="UserRequestMapper"/>
    /// </summary>
    /// <param name="mapper">Auto mapper</param>
    public UserRequestMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public UserRequest MapToModel(UserRequestEntity entity)
    {
        return _mapper.Map<UserRequest>(entity);
    }

    /// <inheritdoc/>
    public UserRequestEntity MapToEntity(UserRequest model)
    {
        return _mapper.Map<UserRequestEntity>(model);
    }

    /// <inheritdoc/>
    public IEnumerable<UserRequest> MapToModelCollection(IEnumerable<UserRequestEntity> entities)
    {
        var collection = new List<UserRequest>();

        foreach (var entity in entities)
        {
            collection.Add(MapToModel(entity));
        }

        return collection;
    }

    /// <inheritdoc/>
    public IEnumerable<UserRequestEntity> MapToEntityCollection(IEnumerable<UserRequest> models)
    {
        var entities = new List<UserRequestEntity>();

        foreach (var model in models)
        {
            entities.Add(MapToEntity(model));
        }

        return entities;
    }
}

public class FileRequestMapper : IFileRequestMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// <see cref="UserRequestMapper"/>
    /// </summary>
    /// <param name="mapper">Auto mapper</param>
    public FileRequestMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public FileRequest MapToModel(FileRequestEntity entity)
    {
        return _mapper.Map<FileRequest>(entity);
    }

    /// <inheritdoc/>
    public FileRequestEntity MapToEntity(FileRequest model)
    {
        return _mapper.Map<FileRequestEntity>(model);
    }

    /// <inheritdoc/>
    public IEnumerable<FileRequest> MapToModelCollection(IEnumerable<FileRequestEntity> entities)
    {
        var collection = new List<FileRequest>();

        foreach (var entity in entities)
        {
            collection.Add(MapToModel(entity));
        }

        return collection;
    }

    /// <inheritdoc/>
    public IEnumerable<FileRequestEntity> MapToEntityCollection(IEnumerable<FileRequest> models)
    {
        var entities = new List<FileRequestEntity>();

        foreach (var model in models)
        {
            entities.Add(MapToEntity(model));
        }

        return entities;
    }
}

/// <inheritdoc/>
public class FileClientIdentityMapper : IFileClientIdentityMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// <see cref="ClientIdentityRequestMapper"/>
    /// </summary>
    /// <param name="mapper">Auto mapper</param>
    public FileClientIdentityMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public FileClientIdentity MapToModel(ClientIdentityRequestEntity entity)
    {
        return _mapper.Map<FileClientIdentity>(entity);
    }

    /// <inheritdoc/>
    public ClientIdentityRequestEntity MapToEntity(FileClientIdentity model)
    {
        return _mapper.Map<ClientIdentityRequestEntity>(model);
    }

    /// <inheritdoc/>
    public IEnumerable<FileClientIdentity> MapToModelCollection(IEnumerable<ClientIdentityRequestEntity> entities)
    {
        var collection = new List<FileClientIdentity>();

        foreach (var entity in entities)
        {
            collection.Add(MapToModel(entity));
        }

        return collection;
    }

    /// <inheritdoc/>
    public IEnumerable<ClientIdentityRequestEntity> MapToEntityCollection(IEnumerable<FileClientIdentity> models)
    {
        var entities = new List<ClientIdentityRequestEntity>();

        foreach (var model in models)
        {
            entities.Add(MapToEntity(model));
        }

        return entities;
    }
}
public class ServiceAccountMapper : IServiceAccountMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// <see cref="ServiceAccountMapper"/>
    /// </summary>
    /// <param name="mapper">Auto mapper</param>
    public ServiceAccountMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public ServiceAccount MapToModel(ServiceAccountEntity entity)
    {
        return _mapper.Map<ServiceAccount>(entity);
    }

    /// <inheritdoc/>
    public ServiceAccountEntity MapToEntity(ServiceAccount model)
    {
        return _mapper.Map<ServiceAccountEntity>(model);
    }

    /// <inheritdoc/>
    public IEnumerable<ServiceAccount> MapToModelCollection(IEnumerable<ServiceAccountEntity> entities)
    {
        var collection = new List<ServiceAccount>();

        foreach (var entity in entities)
        {
            collection.Add(MapToModel(entity));
        }

        return collection;
    }

    /// <inheritdoc/>
    public IEnumerable<ServiceAccountEntity> MapToEntityCollection(IEnumerable<ServiceAccount> models)
    {
        var entities = new List<ServiceAccountEntity>();

        foreach (var model in models)
        {
            entities.Add(MapToEntity(model));
        }

        return entities;
    }
}