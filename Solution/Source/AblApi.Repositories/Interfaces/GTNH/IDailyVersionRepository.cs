using AblApi.DataAccess.Models.GTNH;

namespace AblApi.Repositories.Interfaces.GTNH;

public interface IDailyVersionRepository : IGenericRepository<DailyVersion>
{
    public Task<DailyVersion?> GetByRunNumberAsync(int runNumber);

    public Task<DailyVersion> GetLatestAsync();
}
