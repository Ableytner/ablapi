using AblApi.Common.Enums;
using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models;
using AblApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Repositories;

public class ApiAccessRoleGrantRepository(AblContext context) : GenericRepository<ApiAccessRoleGrant>(context), IApiAccessRoleGrantRepository
{
    public async Task DeleteGrantByRoleAsync(Guid userId, ApiAccessRole role)
    {
        await Context.ApiAccessRoleGrants
            .Where(g =>
                g.UserId == userId &&
                g.Role == role)
            .ExecuteDeleteAsync();
    }
}
