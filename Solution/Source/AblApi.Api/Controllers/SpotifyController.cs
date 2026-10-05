using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using AblApi.Core.AppSpotify;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class SpotifyController(SpotifyClientFactory spotifyClientFactory) : ControllerBase
{
    private readonly SpotifyClientFactory _spotifyClientFactory = spotifyClientFactory;

    [HttpGet("oauth/begin")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [EndpointSummary("Start authenticating with Spotify")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> BeginOAuth()
    {
        if (_spotifyClientFactory.Instance != null)
        {
            return Forbid();
        }

        var redirectUrl = await _spotifyClientFactory.GetAuthUrl();

        return Redirect(redirectUrl);
    }

    [HttpGet("oauth/complete")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [EndpointSummary("Finish authenticating with Spotify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CompleteOAuth([FromQuery] string code)
    {
        if (_spotifyClientFactory.Instance != null)
        {
            return Forbid();
        }

        if (!await _spotifyClientFactory.Build(code))
        {
            return Forbid();
        }

        return Content("Your account was successfully linked\n\nYou can close this tab now");
    }
}
