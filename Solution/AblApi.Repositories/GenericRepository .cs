using AblApi.DataAccess.Context;
using AblApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AblApi.Repositories;

public class GenericRepository<T>(AblContext context) : IGenericRepository<T> where T : class
{
    protected readonly AblContext Context = context;

    // ------------------------------------------------------------
    // Section Getters
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await Context.Set<T>().FindAsync(id);
    }
    public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> expression)
    {
        return await Context.Set<T>().Where(expression).FirstOrDefaultAsync();
    }

    public IQueryable<T> GetAll()
    {
        return Context.Set<T>().AsQueryable();
    }

    public async Task<List<T>> GetAllAsListAsync()
    {
        return await Context.Set<T>().ToListAsync();
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> expression)
    {
        return Context.Set<T>().Where(expression);
    }

    // ------------------------------------------------------------
    // Section Adders
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

    // ------------------------------------------------------------
    // Section Removers
    public void Remove(T entity)
    {
        Context.Set<T>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        Context.Set<T>().RemoveRange(entities);
    }
}