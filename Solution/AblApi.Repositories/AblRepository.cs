using AblApi.DataAccess.Context;
using AblApi.Repositories.Interfaces;
using AblApi.Repositories.Interfaces.GTNH;
using Microsoft.EntityFrameworkCore.Storage;

namespace AblApi.Repositories;

public class AblRepository : IAblRepository
{
    public AblRepository(AblContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));

        ApiUserRepository = new ApiUserRepository(Context);
        ApiAccessRoleGrantRepository = new ApiAccessRoleGrantRepository(Context);
    }

    public readonly AblContext Context;

    public IApiUserRepository ApiUserRepository { get; }

    public IApiAccessRoleGrantRepository ApiAccessRoleGrantRepository { get; }

    public IStableVersionRepository GTNHStableVersionRepository { get; }

    public IDailyVersionRepository GTNHDailyVersionRepository { get; }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await Context.Database.BeginTransactionAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
