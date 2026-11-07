using AblApi.Common.Utilities;
using AblApi.Core.AppSpotify;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Api;

public class SpotifyLiveTests : IAsyncLifetime
{
    private readonly SpotifyTestApiFactory _factory;
    private readonly ISpotifyService _service;

    public SpotifyLiveTests()
    {
        EnvironmentHelper.SetEnvironment("Development");

        _factory = new SpotifyTestApiFactory();
        _service = _factory.Services.GetRequiredService<ISpotifyService>();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _factory.SeedContext.DisposeAsync();
    }

//    [Fact]
    [Fact(Skip = "Live API test")]
    public async Task CRUDPlaylistAsync_Works()
    {
        // Arrange
        var user = SpotifyTestApiFactory.TestUser;
        var playlistName = "TestPlaylist";
        var trackUri ="spotify:track:2A7GGXmTlXuH9LOvBXgOX4";
        var userPlaylistsCount = (await _service.GetAllPlaylists(user, TestContext.Current.CancellationToken)).Count();

        // Act
        var playlist = await _service.CreatePlaylist(user, playlistName, "", false, TestContext.Current.CancellationToken);
        Assert.Equal(0, (await _service.GetPlaylist(user, playlist.Id, TestContext.Current.CancellationToken)).TrackCount);
        await _service.AddTracks(user, playlist.Id, [trackUri], TestContext.Current.CancellationToken);
        Assert.Equal(1, (await _service.GetPlaylist(user, playlist.Id, TestContext.Current.CancellationToken)).TrackCount);
        await _service.RemoveTracks(user, playlist.Id, [trackUri], TestContext.Current.CancellationToken);
        Assert.Equal(0, (await _service.GetPlaylist(user, playlist.Id, TestContext.Current.CancellationToken)).TrackCount);
        await _service.DeletePlaylist(user, playlist.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(userPlaylistsCount, (await _service.GetAllPlaylists(user, TestContext.Current.CancellationToken)).Count());
    }
}
