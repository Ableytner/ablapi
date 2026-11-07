using AblApi.Core.AppSpotify.Domain;

namespace AblApi.Core.AppSpotify;

public interface IAllPlaylistService
{
    Task UpdateAsync(string user, CancellationToken cancellationToken = default);

    Task<Playlist> RegisterAsync(string user, CancellationToken cancellationToken = default);
}
