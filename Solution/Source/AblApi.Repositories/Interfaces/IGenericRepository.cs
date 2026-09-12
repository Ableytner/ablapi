using System.Linq.Expressions;

namespace AblApi.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    // ------------------------------------------------------------
    // Section Getters
    public Task<T?> GetByIdAsync(Guid id);
    public Task<T?> GetByIdAsync(Expression<Func<T, bool>> expression);

    public IQueryable<T> GetAll();

    public Task<List<T>> GetAllAsListAsync();
    
    public IQueryable<T> Find(Expression<Func<T, bool>> expression);

    // ------------------------------------------------------------
    // Section Adders
    public void Add(T entity);

    public void AddRange(IEnumerable<T> entities);

    public Task AddRangeAsync(IEnumerable<T> entities);

    // ------------------------------------------------------------
    // Section Removers
    public void Remove(T entity);

    public void RemoveRange(IEnumerable<T> entities);
}
