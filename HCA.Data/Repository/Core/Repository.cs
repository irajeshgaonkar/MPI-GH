using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HCA.Data.Repository.Core;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    private readonly DbContext _dataBaseContext;

    public RepositoryBase(DbContext context)
    {
        _dataBaseContext = context;
    }

    public IQueryable<T> GetAll()
    {
        return GetAll(null, null, null, null, null);
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
    {
        return GetAll(predicate, null, null, null, null);
    }

    public bool Any(Expression<Func<T, bool>> predicate)
    {
        IQueryable<T> query = _dataBaseContext.Set<T>();
        var result = query.Any(predicate);
        return result;
    }

    public bool All(Expression<Func<T, bool>> predicate)
    {
        IQueryable<T> query = _dataBaseContext.Set<T>();
        var result = query.All(predicate);
        return result;
    }

    public IQueryable<T> GetAll(Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
    {
        return GetAll(null, include, null, null, null);
    }

    public Task<IQueryable<T>> GetAllAsync()
    {
        return GetAllAsync(null, null, null, null, null);
    }

    public Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        return GetAllAsync(predicate, null, null, null, null);
    }

    public Task<IQueryable<T>> GetAllAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>> include)
    {
        return GetAllAsync(null, include, null, null, null);
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        return GetQueryable(predicate, include);
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>>? predicate = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
      Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
      int? skip = null, int? take = null)
    {
        IQueryable<T> query = GetQueryable(predicate, include);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (skip != null && skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take != null && take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return query;
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>>? predicate = null,
                                 Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                 int? skip = null, int? take = null)
    {
        IQueryable<T> query = GetQueryable(predicate, include);

        //if (orderBy != null)
        //{
        //    query = query.OrderBy(orderBy, orderDirection);
        //}

        if (skip != null && skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take != null && take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return query;
    }

    public Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
                                             Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                             Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                             int? skip = null, int? take = null)
    {
        IQueryable<T> query = GetQueryable(predicate, include);

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (skip != null && skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take != null && take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return Task.FromResult(query);
    }

    public Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
                                            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        return Task.FromResult(GetQueryable(predicate, include));
    }

    public Task<IQueryable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
     Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
     int? skip = null, int? take = null)
    {
        IQueryable<T> query = GetQueryable(predicate, include);

        //if (orderBy != null)
        //{
        //    query = query.OrderBy(orderBy, orderDirection);
        //}

        if (skip != null && skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take != null && take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return Task.FromResult(query);
    }

    /// <summary>
    /// Returns a single instance of T but throws exception if none is found
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public T? GetSingle(
     Expression<Func<T, bool>>? predicate = null,
     Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = GetQueryable(predicate, include);

        return query?.FirstOrDefault();
    }

    public async Task<T?> GetSingleAsync(
      Expression<Func<T, bool>>? predicate = null,
      Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = GetQueryable(predicate, include);
        return await Task.FromResult(query.FirstOrDefault());
    }

    public virtual void Add(T entity)
    {
        _dataBaseContext.Set<T>().Add(entity);
        _dataBaseContext.SaveChanges();
    }

    public virtual void AddAsync(T entity)
    {
        _dataBaseContext.Set<T>().AddAsync(entity);
        _dataBaseContext.SaveChanges();
    }

    public T Update(T entity)
    {
        _dataBaseContext.Set<T>().Update(entity);
        _dataBaseContext.SaveChanges();
        return entity;
    }

    public void Delete(Expression<Func<T, bool>> predicate)
    {
        var entity = GetSingle(predicate: predicate);
        if (entity == null) return;
        _dataBaseContext.Set<T>().Remove(entity);
        _dataBaseContext.SaveChanges();
    }

    public void Delete(T entity)
    {
        _dataBaseContext.Set<T>().Remove(entity);
        _dataBaseContext.SaveChanges();
    }

    public int Count()
    {
        return _dataBaseContext.Set<T>().Count();
    }

    public int Count(Expression<Func<T, bool>> predicate)
    {
        return _dataBaseContext.Set<T>().Count(predicate);
    }

    private IQueryable<T> GetQueryable(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = _dataBaseContext.Set<T>();

        if (include != null)
        {
            query = include(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query;
    }
}
