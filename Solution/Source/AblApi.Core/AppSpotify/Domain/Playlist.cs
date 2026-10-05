using SpotifyAPI.Web;

namespace AblApi.Core.AppSpotify.Domain;

public class Playlist
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string OwnerId { get; set; }
    public required string SnapshotId { get; set; }
    public required int TrackCount { get; set; }
    public required IEnumerable<PlaylistTrack> Tracks { get; set; }

    public string Uri { get => $"spotify:playlist:{Id}"; }

    public static Playlist FromDto(FullPlaylist dto)
    {
        return new Playlist()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            OwnerId = dto.Owner.Id,
            SnapshotId = dto.SnapshotId,
            TrackCount = dto.Items.Total.Value,
            Tracks = []
        };
    }
}
