using SpotifyAPI.Web;

namespace AblApi.Core.AppSpotify.Domain;

public class Track
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string[] Artists { get; set; }
    public required bool IsLocal { get; set; }

    public string Uri { get => $"spotify:track:{Id}"; }

    public static Track FromDto(FullTrack dto)
    {
        return new Track()
        {
            Id = dto.Id,
            Name = dto.Name,
            Artists = dto.Artists.Select(x => x.Name).ToArray(),
            IsLocal = dto.IsLocal
        };
    }
}
