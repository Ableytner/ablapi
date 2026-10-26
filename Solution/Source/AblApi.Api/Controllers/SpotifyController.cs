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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> BeginOAuth([FromQuery] string user)
    {
        if (_spotifyClientFactory.HasClient(user))
        {
            return Content($"The user '{user}' already has a linked Spotify account.");
        }

        var redirectUrl = await _spotifyClientFactory.GetAuthUrl(user);

        return Redirect(redirectUrl);
    }

    [HttpGet("oauth/complete")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [EndpointSummary("Finish authenticating with Spotify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CompleteOAuth([FromQuery] string code, [FromQuery] string state)
    {
        // state is the user identifier previously sent, echoed back by Spotify
        if (_spotifyClientFactory.HasClient(state))
        {
            return Content($"The user '{state}' already has a linked Spotify account.");
        }

        if (!await _spotifyClientFactory.Build(code, state))
        {
            return Forbid();
        }

        return Content("Your account was successfully linked\n\nYou can close this tab now");
    }
}
