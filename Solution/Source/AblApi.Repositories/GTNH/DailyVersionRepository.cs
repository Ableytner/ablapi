using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.GTNH;
using AblApi.Repositories.Interfaces.GTNH;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Repositories.GTNH;

public class DailyVersionRepository(AblContext context) : GenericRepository<DailyVersion>(context), IDailyVersionRepository
{
    public async Task<DailyVersion?> GetByRunNumberAsync(int runNumber)
    {
        return await Context.GTNHDailyVersions.FirstOrDefaultAsync(x => x.RunNumber == runNumber);
    }

    public async Task<DailyVersion?> GetLatestAsync()
    {
        return await Context.GTNHDailyVersions.OrderByDescending(x => x.RunNumber).FirstOrDefaultAsync();
    }

    public async new Task<List<DailyVersion>> GetAllAsListAsync()
    {
        return await Context.GTNHDailyVersions.OrderByDescending(x => x.RunNumber).ToListAsync();
    }
}
