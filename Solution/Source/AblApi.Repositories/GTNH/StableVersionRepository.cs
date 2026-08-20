using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.GTNH;
using AblApi.Repositories.Interfaces.GTNH;
using Microsoft.EntityFrameworkCore;
using AblApi.Common.Extensions;

namespace AblApi.Repositories.GTNH;

public class StableVersionRepository(AblContext context) : GenericRepository<StableVersion>(context), IStableVersionRepository
{
    public async new Task<List<StableVersion>> GetAllAsListAsync()
    {
        var stableVersions = await Context.GTNHStableVersions.ToListAsync();
        return stableVersions.SortByVersionDescending(x => x.Version);
    }
}
