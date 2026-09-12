using AblApi.Repositories.Interfaces.GTNH;
using Microsoft.EntityFrameworkCore.Storage;

namespace AblApi.Repositories.Interfaces;

public interface IAblRepository : IDisposable
{
    IApiUserRepository ApiUserRepository { get; }

    IApiAccessRoleGrantRepository ApiAccessRoleGrantRepository { get; }

    IStableVersionRepository GTNHStableVersionRepository { get; }

    IDailyVersionRepository GTNHDailyVersionRepository { get; }

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<int> SaveChangesAsync();
}
