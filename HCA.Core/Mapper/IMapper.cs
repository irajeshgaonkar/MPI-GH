namespace HCA.Core.Mapper;

/// <summary>
/// Mapper interface for mapping entity to model and vice verst
/// </summary>
/// <typeparam name="E">Entity</typeparam>
/// <typeparam name="M">Model</typeparam>
public interface IMapper<E, M>
{
    /// <summary>
    /// Map to model from Entity
    /// </summary>
    /// <param name="entity">Entity object</param>
    /// <returns>Mapped model object</returns>
    M MapToModel(E entity);

    /// <summary>
    /// Map to entity from model
    /// </summary>
    /// <param name="model">Model objecg</param>
    /// <returns>Mapped entity object</returns>
    E MapToEntity(M model);

    /// <summary>
    /// Map to model collection from entity collection
    /// </summary>
    /// <param name="entities">Collection of entity objects</param>
    /// <returns>Collection of mapped model objects</returns>
    IEnumerable<M> MapToModelCollection(IEnumerable<E> entities);

    /// <summary>
    /// Map to entity collection from model collection
    /// </summary>
    /// <param name="models">Collection of model objects</param>
    /// <returns>Collection of mapped entity objects</returns>
    IEnumerable<E> MapToEntityCollection(IEnumerable<M> models);
}

