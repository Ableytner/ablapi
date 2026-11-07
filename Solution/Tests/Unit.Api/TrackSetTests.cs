using AblApi.Core.AppSpotify.Domain;

namespace Tests.Unit.Api;

public class TrackSetTests
{
    private static PlaylistTrack Track(string id, DateTime addedAt) => new()
    {
        Id = id,
        Name = $"name-{id}",
        Artists = ["artist"],
        IsLocal = false,
        AddedAt = addedAt,
    };

    [Fact]
    public void Add_EmptySet_CountIsZero()
    {
        var set = new TrackSet();

        Assert.Equal(0, set.Count);
    }

    [Fact]
    public void Add_SingleTrack_CountIsOne()
    {
        var set = new TrackSet();
        set.Add(Track("a", new DateTime(2020, 1, 1)));

        Assert.Equal(1, set.Count);
    }

    [Fact]
    public void Add_DuplicateId_KeepsSingleEntry()
    {
        var set = new TrackSet();
        set.Add(Track("a", new DateTime(2020, 1, 1)));
        set.Add(Track("a", new DateTime(2021, 1, 1)));

        Assert.Equal(1, set.Count);
    }

    [Fact]
    public void Add_DuplicateId_KeepsOldestTimestamp()
    {
        var older = new DateTime(2019, 5, 5);
        var newer = new DateTime(2021, 5, 5);

        var set = new TrackSet();
        set.Add(Track("a", newer));
        set.Add(Track("a", older));

        var tracks = set.Tracks();

        var item = Assert.Single(tracks);
        Assert.Equal(older, item.AddedAt);
    }

    [Fact]
    public void Add_DuplicateId_NewerTimestampDoesNotOverrideOlder()
    {
        var older = new DateTime(2019, 5, 5);
        var newer = new DateTime(2021, 5, 5);

        var set = new TrackSet();
        set.Add(Track("a", older));
        set.Add(Track("a", newer));

        var tracks = set.Tracks();

        var item = Assert.Single(tracks);
        Assert.Equal(older, item.AddedAt);
    }

    [Fact]
    public void Tracks_ReturnsOldestToNewest()
    {
        var set = new TrackSet();
        set.Add(Track("c", new DateTime(2022, 1, 1)));
        set.Add(Track("a", new DateTime(2020, 1, 1)));
        set.Add(Track("b", new DateTime(2021, 1, 1)));

        var ids = set.Tracks().Select(t => t.Id).ToArray();

        Assert.Equal(["a", "b", "c"], ids);
    }

    [Fact]
    public void Ids_ReturnsOldestToNewest()
    {
        var set = new TrackSet();
        set.Add(Track("c", new DateTime(2022, 1, 1)));
        set.Add(Track("a", new DateTime(2020, 1, 1)));
        set.Add(Track("b", new DateTime(2021, 1, 1)));

        Assert.Equal(["a", "b", "c"], set.Ids());
    }

    [Fact]
    public void Ids_MatchesTracksIds()
    {
        var set = new TrackSet();
        set.Add(Track("x", new DateTime(2020, 3, 3)));
        set.Add(Track("y", new DateTime(2019, 3, 3)));
        set.Add(Track("z", new DateTime(2021, 3, 3)));

        Assert.Equal(set.Tracks().Select(t => t.Id), set.Ids());
    }

    [Fact]
    public void Add_MultipleDistinctTracks_AllPresent()
    {
        var set = new TrackSet();
        set.Add(Track("a", new DateTime(2020, 1, 1)));
        set.Add(Track("b", new DateTime(2020, 2, 1)));
        set.Add(Track("c", new DateTime(2020, 3, 1)));

        Assert.Equal(3, set.Count);
        Assert.Equal(["a", "b", "c"], set.Ids());
    }

    [Fact]
    public void Add_SameTrackAcrossPlaylists_DeduplicatesByOldest()
    {
        var set = new TrackSet();
        set.Add(Track("song", new DateTime(2021, 6, 1)));
        set.Add(Track("song", new DateTime(2020, 6, 1)));
        set.Add(Track("other", new DateTime(2020, 1, 1)));

        var tracks = set.Tracks();

        Assert.Equal(2, tracks.Count);
        var song = tracks.Single(t => t.Id == "song");
        Assert.Equal(new DateTime(2020, 6, 1), song.AddedAt);
    }
}
