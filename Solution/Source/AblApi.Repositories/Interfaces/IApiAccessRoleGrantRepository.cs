using AblApi.Common.Enums;

namespace AblApi.Repositories.Interfaces;

public interface IApiAccessRoleGrantRepository
{
    public Task DeleteGrantByRoleAsync(Guid userId, ApiAccessRole role);
}
