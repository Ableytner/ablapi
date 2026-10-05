using AblApi.Core.AppSpotify.Exceptions;
using SpotifyAPI.Web;

namespace AblApi.Core.AppSpotify.Domain;

public class PlaylistTrack : Track
{
    public required DateTime AddedAt { get; set; }

    public static PlaylistTrack FromDto(PlaylistTrack<IPlayableItem> dtoRaw)
    {
        if (dtoRaw.Item is FullTrack dto)
        {
            return new PlaylistTrack()
            {
                Id = dto.Id,
                Name = dto.Name,
                Artists = dto.Artists.Select(x => x.Name).ToArray(),
                IsLocal = dto.IsLocal,
                AddedAt = dtoRaw.AddedAt!.Value
            };
        }

        throw new UnsupportedTrackTypeException();
    }
    public static PlaylistTrack FromDto(SavedTrack dtoRaw)
    {
        var dto = dtoRaw.Track;

        return new PlaylistTrack()
        {
            Id = dto.Id,
            Name = dto.Name,
            Artists = dto.Artists.Select(x => x.Name).ToArray(),
            IsLocal = dto.IsLocal,
            AddedAt = dtoRaw.AddedAt
        };
    }
}
