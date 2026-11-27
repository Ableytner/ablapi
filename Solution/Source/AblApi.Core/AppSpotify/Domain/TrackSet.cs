namespace AblApi.Core.AppSpotify.Domain;

public class TrackSet
{
    private readonly Dictionary<string, DateTime> _tracks = new();

    public int Count => _tracks.Count;

    public void Add(PlaylistTrack track)
    {
        if (!_tracks.TryGetValue(track.Id, out var existing))
        {
            _tracks[track.Id] = track.AddedAt;
        }
        else if (track.AddedAt < existing)
        {
            _tracks[track.Id] = track.AddedAt;
        }
    }

    public List<PlaylistTrack> Tracks()
    {
        return _tracks
            .Select(kv => new PlaylistTrack
            {
                Id = kv.Key,
                Name = string.Empty,
                Artists = [],
                IsLocal = false,
                AddedAt = kv.Value,
            })
            .OrderBy(t => t.AddedAt)
            .ToList();
    }

    public List<string> Ids()
    {
        return _tracks
            .OrderBy(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();
    }
}
