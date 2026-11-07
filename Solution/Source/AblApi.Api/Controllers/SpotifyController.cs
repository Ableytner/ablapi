using System.Security.Claims;
using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken.Attributes;
using AblApi.Core.AppSpotify;
using AblApi.Core.AppSpotify.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AblApi.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class SpotifyController(SpotifyClientFactory spotifyClientFactory, IAllPlaylistService allPlaylistService) : ControllerBase
{
    private readonly SpotifyClientFactory _spotifyClientFactory = spotifyClientFactory;
    private readonly IAllPlaylistService _allPlaylistService = allPlaylistService;

    [HttpGet("oauth/begin")]
    [AuthorizeSpotify]
    [EndpointSummary("Start authenticating with Spotify")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> BeginOAuth()
    {
        var user = GetCurrentUserId();

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

    [HttpPost("everything/register")]
    [AuthorizeSpotify]
    [EndpointSummary("Register the current user for the everything playlist")]
    [EndpointDescription("Creates the private '🌎 everything' playlist on first use and fills it with all tracks from all of the user's playlists and liked songs.")]
    [ProducesResponseType(typeof(EverythingPlaylistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EverythingPlaylistDto>> RegisterEverything()
    {
        var user = GetCurrentUserId();

        var playlist = await _allPlaylistService.RegisterAsync(user, HttpContext.RequestAborted);

        return Ok(EverythingPlaylistDto.FromDomain(playlist));
    }

    private string GetCurrentUserId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (claim is null)
        {
            throw new InvalidOperationException("Authenticated user has no subject claim.");
        }

        return claim.Value;
    }
}
