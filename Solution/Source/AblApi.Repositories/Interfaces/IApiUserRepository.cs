using AblApi.DataAccess.Models;

namespace AblApi.Repositories.Interfaces;

public interface IApiUserRepository : IGenericRepository<ApiUser>
{
    public Task UpsertAsync(ApiUser entity);
}
