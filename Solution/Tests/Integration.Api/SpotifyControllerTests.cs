using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;

namespace Integration.Api;

public class SpotifyControllerTests : RealAuthTestBase
{
    private const string RegisterEndpoint = "spotify/everything/register";

    private readonly IJwtTokenService _jwtTokenService;

    public SpotifyControllerTests()
    {
        _jwtTokenService = TestHelpers.ApiFactory.Services.GetRequiredService<IJwtTokenService>();
    }

    [Fact]
    public async Task Register_UnlinkedUser_Returns404()
    {
        // Arrange: a valid Spotify-role token, but no linked Spotify account for this user
        var token = _jwtTokenService.CreateToken(Guid.NewGuid(), [ApiAccessRole.Spotify]);

        // Act
        var response = await SendAsync(token.Token);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Register_MissingToken_Returns401()
    {
        // Act
        var response = await SendAsync(null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_TokenWithoutSpotifyRole_Returns403()
    {
        // Arrange: authenticated, but without the Spotify role
        var token = _jwtTokenService.CreateToken(Guid.NewGuid(), []);

        // Act
        var response = await SendAsync(token.Token);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAsync(string? bearerToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, RegisterEndpoint);
        if (bearerToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return await TestHelpers.Client.SendAsync(request, CancellationToken);
    }
}
