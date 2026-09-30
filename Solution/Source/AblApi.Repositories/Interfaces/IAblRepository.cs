using AblApi.Repositories.Interfaces.GTNH;
using AblApi.Repositories.Interfaces.Willhaben;
using Microsoft.EntityFrameworkCore.Storage;

namespace AblApi.Repositories.Interfaces;

public interface IAblRepository : IDisposable
{
    IApiUserRepository ApiUserRepository { get; }

    IApiAccessRoleGrantRepository ApiAccessRoleGrantRepository { get; }

    IStableVersionRepository GTNHStableVersionRepository { get; }

    IDailyVersionRepository GTNHDailyVersionRepository { get; }

    IWillhabenConfigRepository WillhabenConfigRepository { get; }

    IWillhabenSeenListingRepository WillhabenSeenListingRepository { get; }

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<int> SaveChangesAsync();
}
