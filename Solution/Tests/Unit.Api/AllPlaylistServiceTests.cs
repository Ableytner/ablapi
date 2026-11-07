using AblApi.Core.AppSpotify;
using AblApi.Core.AppSpotify.Domain;
using AblApi.Core.AppSpotify.Exceptions;
using AblApi.DataAccess.Models.Spotify;
using AblApi.Repositories.Interfaces;
using AblApi.Repositories.Interfaces.Spotify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Unit.Api.Mocks;

namespace Tests.Unit.Api;

public class AllPlaylistServiceTests(InMemorySqliteDbFixture fixture) : IClassFixture<InMemorySqliteDbFixture>
{
    private readonly InMemorySqliteDbFixture _fixture = fixture;
    private readonly IAblRepository _ablRepository = fixture.ServiceProvider.GetRequiredService<IAblRepository>();
    private readonly ILogger<AllPlaylistService> _logger = Substitute.For<ILogger<AllPlaylistService>>();
    private readonly ISpotifyService _spotify = Substitute.For<ISpotifyService>();

    [Fact]
    public async Task UpdateAsync_UserNotFound_DoesNothing()
    {
        // Arrange: no SpotifyUser row for this identifier
        var service = CreateService();

        // Act
        await service.UpdateAsync("user-not-found", TestContext.Current.CancellationToken);

        // Assert: no Spotify API calls at all
        await _spotify.DidNotReceiveWithAnyArgs().GetAllPlaylists(Arg.Any<string>(), TestContext.Current.CancellationToken);
        await _spotify.DidNotReceiveWithAnyArgs().AddTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_NoAllPlaylistId_DoesNothing()
    {
        // Arrange: user exists but has no all_playlist yet
        await SeedUser("no-all-playlist", allPlaylistId: null);
        var service = CreateService();

        // Act
        await service.UpdateAsync("no-all-playlist", TestContext.Current.CancellationToken);

        // Assert
        await _spotify.DidNotReceiveWithAnyArgs().GetAllPlaylists(Arg.Any<string>(), TestContext.Current.CancellationToken);
        await _spotify.DidNotReceiveWithAnyArgs().AddTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_FirstRun_BuildsAllPlaylistFromScratch()
    {
        const string user = "first-run";

        // Arrange
        await SeedUser(user, allPlaylistId: "all1");

        _spotify.GetAllPlaylists(user, Arg.Any<CancellationToken>()).Returns([Playlist(user, "p1", "S1"), Playlist(user, "p2", "S2")]);
        _spotify.GetLikedSongsCount(user, Arg.Any<CancellationToken>()).Returns(1);
        _spotify.GetPlaylist(user, "all1", Arg.Any<CancellationToken>()).Returns(Playlist(user, "all1", null, trackCount: 4));
        // No cache entries -> every source playlist is "changed"
        _spotify.GetPlaylistTracks(user, "p1", Arg.Any<CancellationToken>()).Returns([Track("t1", 1), Track("t2", 2)]);
        _spotify.GetPlaylistTracks(user, "p2", Arg.Any<CancellationToken>()).Returns([Track("t2", 2), Track("t3", 3)]);
        _spotify.GetPlaylistTracks(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, Arg.Any<CancellationToken>()).Returns([Track("t4", 4)]);
        // The all_playlist itself is empty on first run
        _spotify.GetPlaylistTracks(user, "all1", Arg.Any<CancellationToken>()).Returns([]);

        var service = CreateService();

        // Act
        await service.UpdateAsync(user, TestContext.Current.CancellationToken);

        // Assert: nothing removed, all four (de-duplicated) tracks added newest-first
        await _spotify.DidNotReceiveWithAnyArgs().RemoveTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        string[] expected_tracks = ["spotify:track:t4", "spotify:track:t3", "spotify:track:t2", "spotify:track:t1"];
        await _spotify.Received(1).AddTracks(user, "all1", Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(expected_tracks)), Arg.Any<CancellationToken>());

        // The caches were written for every source plus the all_playlist
        Assert.NotNull(await _ablRepository.SpotifyPlaylistCacheRepository.GetByPlaylistIdAsync(user, "p1"));
        Assert.NotNull(await _ablRepository.SpotifyPlaylistCacheRepository.GetByPlaylistIdAsync(user, "p2"));
        Assert.NotNull(await _ablRepository.SpotifyPlaylistCacheRepository.GetByPlaylistIdAsync(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId));
        Assert.NotNull(await _ablRepository.SpotifyPlaylistCacheRepository.GetByPlaylistIdAsync(user, "all1"));
    }

    [Fact]
    public async Task UpdateAsync_AllCachesPresent_EarlyExits()
    {
        const string user = "all-cached";

        // Arrange: prime every cache so the service can prove nothing changed
        await SeedUser(user, allPlaylistId: "all1");
        await SeedCache(user, "p1", "S1", [Track("t1", 1)]);
        await SeedCache(user, "p2", "S2", [Track("t2", 2)]);
        await SeedCache(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, null, [Track("t3", 3)]);
        await SeedCache(user, "all1", null, [Track("t1", 1), Track("t2", 2), Track("t3", 3)]);

        _spotify.GetAllPlaylists(user, Arg.Any<CancellationToken>()).Returns([Playlist(user, "p1", "S1"), Playlist(user, "p2", "S2")]);
        _spotify.GetLikedSongsCount(user, Arg.Any<CancellationToken>()).Returns(1);
        _spotify.GetPlaylist(user, "all1", Arg.Any<CancellationToken>()).Returns(Playlist(user, "all1", null, trackCount: 3));

        var service = CreateService();

        // Act
        await service.UpdateAsync(user, TestContext.Current.CancellationToken);

        // Assert: no track fetches and no mutations
        await _spotify.DidNotReceiveWithAnyArgs().GetPlaylistTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _spotify.DidNotReceiveWithAnyArgs().AddTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        await _spotify.DidNotReceiveWithAnyArgs().RemoveTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_TrackAppendedToSource_AddsIt()
    {
        const string user = "track-added";

        // Arrange: p1 changed (new snapshot) and gained t5, the newest track, so it
        // is appended to the end of the de-duplicated set
        await SeedUser(user, allPlaylistId: "all1");
        await SeedCache(user, "p1", "S1", [Track("t1", 1)]);
        await SeedCache(user, "p2", "S2", [Track("t2", 2)]);
        await SeedCache(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, null, [Track("t4", 4)]);
        await SeedCache(user, "all1", null, [Track("t1", 1), Track("t2", 2), Track("t4", 4)]);

        _spotify.GetAllPlaylists(user, Arg.Any<CancellationToken>()).Returns([Playlist(user, "p1", "S1-new"), Playlist(user, "p2", "S2")]);
        _spotify.GetLikedSongsCount(user, Arg.Any<CancellationToken>()).Returns(1);
        // First call (step 2) reports the current count so the all1 cache is a hit;
        // subsequent poll calls report the final count so the wait loop terminates
        _spotify.GetPlaylist(user, "all1", Arg.Any<CancellationToken>())
            .Returns(
                Playlist(user, "all1", null, trackCount: 3),
                Playlist(user, "all1", null, trackCount: 4));
        _spotify.GetPlaylistTracks(user, "p1", Arg.Any<CancellationToken>()).Returns([Track("t1", 1), Track("t5", 5)]);

        var service = CreateService();

        // Act
        await service.UpdateAsync(user, TestContext.Current.CancellationToken);

        // Assert: only the new track is added; nothing removed
        await _spotify.DidNotReceiveWithAnyArgs().RemoveTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        string[] expected_tracks = ["spotify:track:t5"];
        await _spotify.Received(1).AddTracks(user, "all1", Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(expected_tracks)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_TrackRemovedFromSource_RemovesIt()
    {
        const string user = "track-removed";

        // Arrange: p1 changed (new snapshot) and lost t1
        await SeedUser(user, allPlaylistId: "all1");
        await SeedCache(user, "p1", "S1", [Track("t1", 1), Track("t3", 3)]);
        await SeedCache(user, "p2", "S2", [Track("t2", 2)]);
        await SeedCache(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, null, [Track("t4", 4)]);
        await SeedCache(user, "all1", null, [Track("t1", 1), Track("t2", 2), Track("t3", 3), Track("t4", 4)]);

        _spotify.GetAllPlaylists(user, Arg.Any<CancellationToken>()).Returns([Playlist(user, "p1", "S1-new"), Playlist(user, "p2", "S2")]);
        _spotify.GetLikedSongsCount(user, Arg.Any<CancellationToken>()).Returns(1);
        // First call (step 2) reports the current count so the all1 cache is a hit;
        // subsequent poll calls report the final count so the wait loop terminates
        _spotify.GetPlaylist(user, "all1", Arg.Any<CancellationToken>())
            .Returns(
                Playlist(user, "all1", null, trackCount: 4),
                Playlist(user, "all1", null, trackCount: 3));
        _spotify.GetPlaylistTracks(user, "p1", Arg.Any<CancellationToken>()).Returns([Track("t3", 3)]);

        var service = CreateService();

        // Act
        await service.UpdateAsync(user, TestContext.Current.CancellationToken);

        // Assert: the removed track is dropped; nothing added
        await _spotify.DidNotReceiveWithAnyArgs().AddTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        string[] expected_tracks = ["spotify:track:t1"];
        await _spotify.Received(1).RemoveTracks(user, "all1", Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(expected_tracks)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_DuplicateTrackAcrossPlaylists_AddedOnce()
    {
        const string user = "duplicate-track";

        // Arrange: t2 appears in both p1 and p2; the all_playlist already has it once
        await SeedUser(user, allPlaylistId: "all1");
        await SeedCache(user, "p1", "S1", [Track("t1", 1), Track("t2", 2)]);
        await SeedCache(user, "p2", "S2", [Track("t2", 2), Track("t3", 3)]);
        await SeedCache(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, null, []);
        await SeedCache(user, "all1", null, [Track("t1", 1), Track("t2", 2), Track("t3", 3)]);

        // Force p1 to be re-fetched (changed) so the de-duplication path is exercised
        _spotify.GetAllPlaylists(user, Arg.Any<CancellationToken>()).Returns([Playlist(user, "p1", "S1-new"), Playlist(user, "p2", "S2")]);
        _spotify.GetLikedSongsCount(user, Arg.Any<CancellationToken>()).Returns(0);
        _spotify.GetPlaylist(user, "all1", Arg.Any<CancellationToken>()).Returns(Playlist(user, "all1", null, trackCount: 3));
        _spotify.GetPlaylistTracks(user, "p1", Arg.Any<CancellationToken>()).Returns([Track("t1", 1), Track("t2", 2)]);

        var service = CreateService();

        // Act
        await service.UpdateAsync(user, TestContext.Current.CancellationToken);

        // Assert: the set is unchanged, so no add/remove is issued
        await _spotify.DidNotReceiveWithAnyArgs().AddTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        await _spotify.DidNotReceiveWithAnyArgs().RemoveTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_LikedSongsCountChanged_RefetchesAndAdds()
    {
        const string user = "liked-changed";

        // Arrange: liked songs cache holds 1 track but the count is now 2
        await SeedUser(user, allPlaylistId: "all1");
        await SeedCache(user, "p1", "S1", [Track("t1", 1)]);
        await SeedCache(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, null, [Track("t2", 2)]);
        await SeedCache(user, "all1", null, [Track("t1", 1), Track("t2", 2)]);

        _spotify.GetAllPlaylists(user, Arg.Any<CancellationToken>()).Returns([Playlist(user, "p1", "S1")]);
        _spotify.GetLikedSongsCount(user, Arg.Any<CancellationToken>()).Returns(2);
        // First call (step 2) reports the current count so the all1 cache is a hit;
        // subsequent poll calls report the final count so the wait loop terminates
        _spotify.GetPlaylist(user, "all1", Arg.Any<CancellationToken>())
            .Returns(
                Playlist(user, "all1", null, trackCount: 2),
                Playlist(user, "all1", null, trackCount: 3));
        _spotify.GetPlaylistTracks(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, Arg.Any<CancellationToken>()).Returns([Track("t2", 2), Track("t5", 5)]);

        var service = CreateService();

        // Act
        await service.UpdateAsync(user, TestContext.Current.CancellationToken);

        // Assert: the new liked song is added
        await _spotify.Received(1).GetPlaylistTracks(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, Arg.Any<CancellationToken>());
        await _spotify.DidNotReceiveWithAnyArgs().RemoveTracks(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        string[] expected_tracks = ["spotify:track:t5"];
        await _spotify.Received(1).AddTracks(user, "all1", Arg.Is<IEnumerable<string>>(x => x.SequenceEqual(expected_tracks)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_UserNotFound_ThrowsUserNotLinked()
    {
        // Arrange: no SpotifyUser row for this identifier
        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<UserNotLinkedException>(
            () => service.RegisterAsync("unlinked-user", TestContext.Current.CancellationToken));

        await _spotify.DidNotReceiveWithAnyArgs().CreateAllPlaylistAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_NoAllPlaylistId_CreatesPlaylist()
    {
        const string user = "register-new";

        // Arrange: linked user without an all_playlist yet
        await SeedUser(user, allPlaylistId: null);

        var created = Playlist(user, "all-new", null);
        _spotify.CreateAllPlaylistAsync(user, Arg.Any<CancellationToken>()).Returns(created);
        _spotify.GetPlaylist(user, "all-new", Arg.Any<CancellationToken>()).Returns(created);

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(user, TestContext.Current.CancellationToken);

        // Assert: the playlist was created and returned
        await _spotify.Received(1).CreateAllPlaylistAsync(user, Arg.Any<CancellationToken>());
        Assert.Same(created, result);
    }

    [Fact]
    public async Task RegisterAsync_ExistingAllPlaylistId_DoesNotCreate()
    {
        const string user = "register-existing";

        // Arrange: linked user whose all_playlist is fully cached, so UpdateAsync early-exits
        await SeedUser(user, allPlaylistId: "all1");
        await SeedCache(user, ISpotifyPlaylistCacheRepository.LikedSongsPlaylistId, null, [Track("t1", 1)]);
        await SeedCache(user, "all1", null, [Track("t1", 1)]);

        _spotify.GetAllPlaylists(user, Arg.Any<CancellationToken>()).Returns([]);
        _spotify.GetLikedSongsCount(user, Arg.Any<CancellationToken>()).Returns(1);
        _spotify.GetPlaylist(user, "all1", Arg.Any<CancellationToken>())
            .Returns(Playlist(user, "all1", null, trackCount: 1), Playlist(user, "all1", null, trackCount: 1));

        var service = CreateService();

        // Act
        var result = await service.RegisterAsync(user, TestContext.Current.CancellationToken);

        // Assert: no new playlist was created
        await _spotify.DidNotReceiveWithAnyArgs().CreateAllPlaylistAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.Equal("all1", result.Id);
    }

    // ------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------

    private AllPlaylistService CreateService()
    {
        return new AllPlaylistService(_spotify, _ablRepository, _logger);
    }

    private async Task SeedUser(string user, string? allPlaylistId)
    {
        await _ablRepository.SpotifyUserRepository.UpsertAsync(new SpotifyUser
        {
            UserIdentifier = user,
            SpotifyId = $"spotify-{user}",
            AccessToken = "token",
            RefreshToken = "refresh",
            LinkedAt = DateTime.UtcNow,
            AllPlaylistId = allPlaylistId,
        });
    }

    private async Task SeedCache(string user, string playlistId, string? snapshotId, IEnumerable<PlaylistTrack> tracks)
    {
        await _ablRepository.SpotifyPlaylistCacheRepository.UpsertAsync(
            user,
            playlistId,
            snapshotId,
            tracks.Select(t => new SpotifyPlaylistCacheTrack { TrackId = t.Id, AddedAt = t.AddedAt }));
    }

    private static Playlist Playlist(string user, string id, string? snapshotId, int trackCount = 0)
    {
        return new Playlist
        {
            Id = id,
            Name = id,
            Description = string.Empty,
            OwnerId = user,
            SnapshotId = snapshotId,
            TrackCount = trackCount,
            Tracks = [],
        };
    }

    private static PlaylistTrack Track(string id, int addedAt)
    {
        return new PlaylistTrack
        {
            Id = id,
            Name = id,
            Artists = [],
            IsLocal = false,
            AddedAt = new DateTime(2024, 1, 1).AddDays(addedAt),
        };
    }
}
