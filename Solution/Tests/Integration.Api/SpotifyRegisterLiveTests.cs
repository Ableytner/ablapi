using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AblApi.Common.Enums;
using AblApi.Common.Utilities;
using AblApi.Core.AppJwtToken;
using AblApi.Core.AppSpotify.Dtos;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Api;

public class SpotifyRegisterLiveTests : IAsyncLifetime
{
    private const string RegisterEndpoint = "spotify/everything/register";

    private readonly SpotifyRegisterTestApiFactory _factory;
    private readonly HttpClient _client;
    private readonly IJwtTokenService _jwtTokenService;

    public SpotifyRegisterLiveTests()
    {
        EnvironmentHelper.SetEnvironment("Development");

        _factory = new SpotifyRegisterTestApiFactory();
        _client = _factory.CreateClient();
        _jwtTokenService = _factory.Services.GetRequiredService<IJwtTokenService>();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

//    [Fact]
    [Fact(Skip = "Live API test")]
    public async Task Register_LinkedUser_ReturnsPlaylist()
    {
        // Arrange: authenticate as the api user the linked account was re-keyed under
        var token = _jwtTokenService.CreateToken(SpotifyRegisterTestApiFactory.TestApiUserId, [ApiAccessRole.Spotify]);
        var request = new HttpRequestMessage(HttpMethod.Post, RegisterEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        // Act
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<EverythingPlaylistDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(dto);
        Assert.False(string.IsNullOrEmpty(dto.Id));
        Assert.False(string.IsNullOrEmpty(dto.Uri));
    }
}
