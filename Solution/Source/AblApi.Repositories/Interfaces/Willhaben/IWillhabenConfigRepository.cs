using AblApi.DataAccess.Models.Willhaben;

namespace AblApi.Repositories.Interfaces.Willhaben;

public interface IWillhabenConfigRepository : IGenericRepository<WillhabenConfig>
{
    Task AddAsync(WillhabenConfig entity);

    Task<WillhabenConfig?> GetByNameAsync(string name);

    Task UpsertAsync(WillhabenConfig entity);

    Task RemoveByNameAsync(string name);
}
