using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models;
using AblApi.Repositories.Interfaces;

namespace AblApi.Repositories;

public class ApiUserRepository : GenericRepository<ApiUser>, IApiUserRepository
{
    public ApiUserRepository(AblContext context) : base(context) { }
}
