namespace AblApi.DataAccess.Models.Spotify;

public class SpotifyPlaylistCacheTrack
{
    public int Id { get; set; }

    public int PlaylistCacheId { get; set; }

    public string TrackId { get; set; } = string.Empty;

    public DateTime AddedAt { get; set; }

    public virtual SpotifyPlaylistCache PlaylistCache { get; set; } = null!;
}
