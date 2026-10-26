using AblApi.DataAccess.Models.Spotify;

namespace AblApi.Repositories.Interfaces.Spotify;

public interface ISpotifyUserRepository : IGenericRepository<SpotifyUser>
{
    Task<SpotifyUser?> GetByUserIdentifierAsync(string userIdentifier);

    Task UpsertAsync(SpotifyUser entity);

    Task RemoveByUserIdentifierAsync(string userIdentifier);
}
