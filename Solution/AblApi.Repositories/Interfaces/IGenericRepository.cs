using System.Linq.Expressions;

namespace AblApi.Repositories.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);

    // ------------------------------------------------------------
    // Section Getters
    T GetById(Guid id);

    T GetById(Expression<Func<T, bool>> expression);

    IQueryable<T> GetAll();

    public IEnumerable<T> GetAllAsList();
    
    IEnumerable<T> Find(Expression<Func<T, bool>> expression);

    // ------------------------------------------------------------
    // Section Adders
    void Add(T entity);

    void AddRange(IEnumerable<T> entities);

    Task AddRangeAsync(IEnumerable<T> entities);

    // ------------------------------------------------------------
    // Section Removers
    void Remove(T entity);

    void RemoveRange(IEnumerable<T> entities);
}
