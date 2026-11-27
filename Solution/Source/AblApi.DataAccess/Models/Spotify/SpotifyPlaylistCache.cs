namespace AblApi.DataAccess.Models.Spotify;

public class SpotifyPlaylistCache
{
    public int Id { get; set; }

    public int SpotifyUserId { get; set; }

    public string PlaylistId { get; set; } = string.Empty;

    public string? SnapshotId { get; set; }

    public virtual SpotifyUser SpotifyUser { get; set; } = null!;

    public virtual ICollection<SpotifyPlaylistCacheTrack> Tracks { get; set; } = new List<SpotifyPlaylistCacheTrack>();
}
