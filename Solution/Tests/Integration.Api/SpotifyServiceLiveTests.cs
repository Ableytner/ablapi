using System.Text.Json;
using AblApi.Core.AppSpotify;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Api;

public class SpotifyLiveTests : TestBase
{
    private readonly ISpotifyService _service;

    public SpotifyLiveTests()
    {
        _service = TestHelpers.ApiFactory.Services.GetRequiredService<ISpotifyService>();
    }

//    [Fact]
    [Fact(Skip = "Live API test")]
    public async Task CRUDPlaylistAsync_Works()
    {
        // Arrange
        var playlistName = "TestPlaylist";
        var trackUri ="spotify:track:2A7GGXmTlXuH9LOvBXgOX4";
        var userPlaylistsCount = (await _service.GetAllPlaylists(TestContext.Current.CancellationToken)).Count();

        // Act
        var playlist = await _service.CreatePlaylist(playlistName, "", false, TestContext.Current.CancellationToken);
        Assert.Equal(0, (await _service.GetPlaylist(playlist.Id, TestContext.Current.CancellationToken)).TrackCount);
        await _service.AddTracks(playlist.Id, [trackUri], TestContext.Current.CancellationToken);
        Assert.Equal(1, (await _service.GetPlaylist(playlist.Id, TestContext.Current.CancellationToken)).TrackCount);
        await _service.RemoveTracks(playlist.Id, [trackUri], TestContext.Current.CancellationToken);
        Assert.Equal(0, (await _service.GetPlaylist(playlist.Id, TestContext.Current.CancellationToken)).TrackCount);
        await _service.DeletePlaylist(playlist.Id, TestContext.Current.CancellationToken);

        // Assert
        // not needed
    }
}
