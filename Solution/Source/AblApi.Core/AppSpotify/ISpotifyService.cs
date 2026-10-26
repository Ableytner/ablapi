using AblApi.Core.AppSpotify.Domain;

namespace AblApi.Core.AppSpotify;

public interface ISpotifyService
{
    public Task<User> GetUserAsync(string user, CancellationToken cancellationToken = default);

    public Task<Playlist> CreatePlaylist(string user, string name, string description, bool isPublic, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Playlist>> GetAllPlaylists(string user, CancellationToken cancellationToken = default);

    public Task<Playlist> GetPlaylist(string user, string id, CancellationToken cancellationToken = default);

    public Task<int> GetLikedSongsCount(string user, CancellationToken cancellationToken = default);

    public Task DeletePlaylist(string user, string id, CancellationToken cancellationToken);

    public Task<IEnumerable<PlaylistTrack>> GetPlaylistTracks(string user, string id, CancellationToken cancellationToken);

    public Task<IEnumerable<PlaylistTrack>> GetLikedSongsTracks(string user, CancellationToken cancellationToken);

    public Task AddTracks(string user, string id, IEnumerable<string> uris, CancellationToken cancellationToken);

    public Task RemoveTracks(string user, string id, IEnumerable<string> uris, CancellationToken cancellationToken);
}
