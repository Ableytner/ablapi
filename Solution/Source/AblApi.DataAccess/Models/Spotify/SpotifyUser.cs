namespace AblApi.DataAccess.Models.Spotify;

public class SpotifyUser
{
    public int Id { get; set; }

    public string UserIdentifier { get; set; } = string.Empty;

    public string SpotifyId { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime LinkedAt { get; set; }

    public virtual ICollection<SpotifyPlaylistCache> Playlists { get; set; } = new List<SpotifyPlaylistCache>();
}
