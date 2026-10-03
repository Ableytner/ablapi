using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.Willhaben;
using AblApi.Repositories.Interfaces.Willhaben;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Repositories.Willhaben;

public class WillhabenConfigRepository(AblContext context) : GenericRepository<WillhabenConfig>(context), IWillhabenConfigRepository
{
    public async Task AddAsync(WillhabenConfig entity)
    {
        Context.WillhabenConfigs.Add(entity);
        await Context.SaveChangesAsync();
    }

    public async Task<WillhabenConfig?> GetByNameAsync(string name)
    {
        return await Context.WillhabenConfigs
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task UpsertAsync(WillhabenConfig entity)
    {
        var existing = await Context.WillhabenConfigs
            .FirstOrDefaultAsync(c => c.Name == entity.Name);

        if (existing == null)
        {
            Context.WillhabenConfigs.Add(entity);
        }
        else
        {
            existing.Keyword = entity.Keyword;
            existing.Category = entity.Category;
            existing.Rows = entity.Rows;
            existing.PriceMin = entity.PriceMin;
            existing.PriceMax = entity.PriceMax;
            existing.FilterPaylivery = entity.FilterPaylivery;
            existing.HandoverTypes = entity.HandoverTypes;
            existing.AllowedStates = entity.AllowedStates;
            existing.KmMax = entity.KmMax;
            existing.MustInclude = entity.MustInclude;
            existing.MustExclude = entity.MustExclude;
            existing.SortByDistance = entity.SortByDistance;
            existing.ReferenceLat = entity.ReferenceLat;
            existing.ReferenceLon = entity.ReferenceLon;
            existing.MaxDistanceKm = entity.MaxDistanceKm;
            existing.IsActive = entity.IsActive;
        }

        await Context.SaveChangesAsync();
    }

    public async Task RemoveByNameAsync(string name)
    {
        var entity = await Context.WillhabenConfigs
            .FirstOrDefaultAsync(c => c.Name == name);

        if (entity != null)
        {
            Context.WillhabenConfigs.Remove(entity);
        }

        await Context.SaveChangesAsync();
    }
}
