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
    public ClientIdentityRequest MapToModel(Data.Entities.ClientIdentityRequestEntity entity)
    {
        return _mapper.Map<ClientIdentityRequest>(entity);
    }

    /// <inheritdoc/>
    public ClientIdentityRequestEntity MapToEntity(Models.Request.ClientIdentityRequest model)
    {
        return _mapper.Map<ClientIdentityRequestEntity>(model);
    }

    /// <inheritdoc/>
    public IEnumerable<ClientIdentityRequest> MapToModelCollection(IEnumerable<Data.Entities.ClientIdentityRequestEntity> entities)
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