namespace AblApi.Core.AppSpotify;

public class SpotifyAppSettings
{
    public const string SectionName = "SpotifyConfig";

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string AllPlaylistName { get; set; } = "🌎 everything";

    public int FetchCycleInSeconds { get; set; } = 15 * 60;
}
