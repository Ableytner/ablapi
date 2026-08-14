using Microsoft.EntityFrameworkCore.Storage;

namespace AblApi.Repositories.Interfaces;

public interface IAblRepository
{
    IApiUserRepository ApiUserRepository { get; }

    IApiAccessRoleGrantRepository ApiAccessRoleGrantRepository { get; }

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<int> SaveChangesAsync();
}
