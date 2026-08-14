using AblApi.DataAccess.Context;
using AblApi.Repositories.Interfaces;
using System.Linq.Expressions;

namespace AblApi.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AblContext Context;

    public GenericRepository(AblContext context)
    {
        Context = context;
    }

    public void Add(T entity)
    {
        Context.Set<T>().Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        Context.Set<T>().AddRange(entities);
    }
    public Task AddRangeAsync(IEnumerable<T> entities)
    {
       return  Context.Set<T>().AddRangeAsync(entities);
    }

    public IEnumerable<T> Find(Expression<Func<T, bool>> expression)
    {
        return Context.Set<T>().Where(expression);
    }

    public T GetById(Expression<Func<T, bool>> expression)
    {
        return Context.Set<T>().Where(expression).FirstOrDefault();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        return await Context.Set<T>().FindAsync(id);
    }


    public T GetById(Guid id)
    {
        return Context.Set<T>().Find(id);
    }

    public IQueryable<T> GetAll()
    {
        return Context.Set<T>().AsQueryable();
    }
    public IEnumerable<T> GetAllAsList()
    {
        return Context.Set<T>().ToList();
    }

    public void Remove(T entity)
    {
        Context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        Context.Set<T>().RemoveRange(entities);
    }
}