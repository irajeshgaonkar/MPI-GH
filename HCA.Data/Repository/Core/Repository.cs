using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace HCA.Data.Repository.Core;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected readonly DbContext DataBaseContext;

    public RepositoryBase(DbContext context)
    {
        DataBaseContext = context;
    }

    public IQueryable<T> GetAll()
    {
        return GetAll(null, null, null, null, null);
    }

    public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate)
    {
        return GetAll(predicate, null, null, null, null);
    }

    public bool Any(Expression<Func<T, bool>> condition, Expression<Func<T, bool>> predicate)
    {
        IQueryable<T> query = DataBaseContext.Set<T>();
        var result = query.Where(condition).Any(predicate);
        return result;
    }

    public bool All(Expression<Func<T, bool>> condition, Expression<Func<T, bool>> predicate)
    {
        IQueryable<T> query = DataBaseContext.Set<T>();
        var result = query.Where(condition).All(predicate);
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
        DataBaseContext.Set<T>().Add(entity);
        DataBaseContext.SaveChanges();
    }

    public virtual void AddAsync(T entity)
    {
        DataBaseContext.Set<T>().AddAsync(entity);
        DataBaseContext.SaveChanges();
    }

    public T Update(T entity)
    {
        DataBaseContext.Set<T>().Update(entity);
        DataBaseContext.SaveChanges();
        return entity;
    }

    public void Delete(Expression<Func<T, bool>> predicate)
    {
        var entity = GetSingle(predicate: predicate);
        if (entity == null) return;
        DataBaseContext.Set<T>().Remove(entity);
        DataBaseContext.SaveChanges();
    }

    public void Delete(T entity)
    {
        DataBaseContext.Set<T>().Remove(entity);
        DataBaseContext.SaveChanges();
    }

    public int Count()
    {
        return DataBaseContext.Set<T>().Count();
    }

    public int Count(Expression<Func<T, bool>> predicate)
    {
        return DataBaseContext.Set<T>().Count(predicate);
    }

    private IQueryable<T> GetQueryable(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = DataBaseContext.Set<T>();

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
