using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.Spotify;
using AblApi.Repositories.Interfaces.Spotify;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Repositories.Spotify;

public class SpotifyUserRepository(AblContext context) : GenericRepository<SpotifyUser>(context), ISpotifyUserRepository
{
    public async Task<SpotifyUser?> GetByUserIdentifierAsync(string userIdentifier)
    {
        return await Context.SpotifyUsers
            .FirstOrDefaultAsync(u => u.UserIdentifier == userIdentifier);
    }

    public async Task UpsertAsync(SpotifyUser entity)
    {
        var existing = await Context.SpotifyUsers
            .FirstOrDefaultAsync(u => u.UserIdentifier == entity.UserIdentifier);

        if (existing == null)
        {
            Context.SpotifyUsers.Add(entity);
        }
        else
        {
            existing.SpotifyId = entity.SpotifyId;
            existing.AccessToken = entity.AccessToken;
            existing.RefreshToken = entity.RefreshToken;
            existing.LinkedAt = entity.LinkedAt;
        }

        await Context.SaveChangesAsync();
    }

    public async Task RemoveByUserIdentifierAsync(string userIdentifier)
    {
        var entity = await Context.SpotifyUsers
            .FirstOrDefaultAsync(u => u.UserIdentifier == userIdentifier);

        if (entity != null)
        {
            Context.SpotifyUsers.Remove(entity);
        }

        await Context.SaveChangesAsync();
    }
}
