using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models;
using AblApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Repositories;

public class ApiUserRepository(AblContext context) : GenericRepository<ApiUser>(context), IApiUserRepository
{
    public async Task UpsertAsync(ApiUser entity)
    {
        var record = await Context.ApiUsers.Where(x => x.Id == entity.Id).FirstOrDefaultAsync();

        if (record == null)
        {
            await Context.AddAsync(entity);
            return;
        }
        else
        {
            record.Name = entity.Name;
            record.Roles = entity.Roles;
        }

        await Context.SaveChangesAsync().ConfigureAwait(false);
    }
}
