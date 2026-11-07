using AblApi.Core.AppSpotify.Domain;

namespace AblApi.Core.AppSpotify.Dtos;

public class EverythingPlaylistDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Uri { get; set; } = string.Empty;

    public int TrackCount { get; set; }

    public static EverythingPlaylistDto FromDomain(Playlist playlist)
    {
        return new EverythingPlaylistDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Uri = playlist.Uri,
            TrackCount = playlist.TrackCount
        };
    }
}
