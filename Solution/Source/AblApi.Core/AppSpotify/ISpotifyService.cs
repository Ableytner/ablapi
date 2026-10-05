using AblApi.Core.AppSpotify.Domain;

namespace AblApi.Core.AppSpotify;

public interface ISpotifyService
{
    public Task<User> GetUserAsync(CancellationToken cancellationToken = default);

    public Task<Playlist> CreatePlaylist(string name, string description, bool isPublic, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Playlist>> GetAllPlaylists(CancellationToken cancellationToken = default);

    public Task<Playlist> GetPlaylist(string id, CancellationToken cancellationToken = default);

    public Task<int> GetLikedSongsCount(CancellationToken cancellationToken = default);

    public Task DeletePlaylist(string id, CancellationToken cancellationToken);

    public Task<IEnumerable<PlaylistTrack>> GetPlaylistTracks(string id, CancellationToken cancellationToken);

    public Task<IEnumerable<PlaylistTrack>> GetLikedSongsTracks(CancellationToken cancellationToken);

    public Task AddTracks(string id, IEnumerable<string> uris, CancellationToken cancellationToken);

    public Task RemoveTracks(string id, IEnumerable<string> uris, CancellationToken cancellationToken);
}
