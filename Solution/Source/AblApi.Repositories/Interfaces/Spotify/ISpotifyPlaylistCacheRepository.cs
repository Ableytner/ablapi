using AblApi.DataAccess.Models.Spotify;

namespace AblApi.Repositories.Interfaces.Spotify;

public interface ISpotifyPlaylistCacheRepository : IGenericRepository<SpotifyPlaylistCache>
{
    // Pseudo playlist id used for the "Liked Songs" playlist, which has no real Spotify playlist id
    public const string LikedSongsPlaylistId = "liked_songs";

    Task<SpotifyPlaylistCache?> GetByPlaylistIdAsync(string user, string playlistId);

    Task UpsertAsync(string user, string playlistId, string? snapshotId, IEnumerable<SpotifyPlaylistCacheTrack> tracks);

    Task RemoveByPlaylistIdAsync(string user, string playlistId);

    Task<SpotifyPlaylistCache?> GetLikedSongsAsync(string user);

    Task UpsertLikedSongsAsync(string user, IEnumerable<SpotifyPlaylistCacheTrack> tracks);

    Task RemoveLikedSongsAsync(string user);
}
