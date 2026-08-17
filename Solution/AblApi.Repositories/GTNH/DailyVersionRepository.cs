using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.GTNH;
using AblApi.Repositories.Interfaces.GTNH;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Repositories.GTNH;

public class DailyVersionRepository(AblContext context) : GenericRepository<DailyVersion>(context), IDailyVersionRepository
{
    public async new Task<List<DailyVersion>> GetAllAsListAsync()
    {
        return await Context.GTNHDailyVersions.OrderByDescending(x => x.RunNumber).ToListAsync();
    }

    public async Task<DailyVersion> GetLatestDailyVersionAsync()
    {
        return await Context.GTNHDailyVersions.OrderByDescending(x => x.RunNumber).FirstAsync();
    }
}
