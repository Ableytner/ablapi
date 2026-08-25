using System;
using System.Net.Http.Headers;
using System.Text;
using AblApi.Core.AppJwtToken;
using AblApi.Core.AppJwtToken.Dtos;
using AblApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(ILogger<AuthController> logger, IAblRepository ablRepository, IJwtTokenService jwtTokenService) : ControllerBase
{
    private readonly ILogger<AuthController> _logger = logger;
    private readonly IAblRepository _ablRepository = ablRepository;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

    [HttpPost]
    [EndpointSummary("Authenticate an ApiUser and issue a JWT token")]
    [EndpointDescription("Expects the Authorization header to contain userId:password encoded in base64, e.g. \"Authorization: Basic <encoded string>\".")]
    [ProducesResponseType(typeof(JwtTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<JwtTokenDto>> Authenticate()
    {
        Guid userId = Guid.Empty;
        try
        {
            if (!TryParseBasicAuthHeader(out userId, out var token))
            {
                _logger.LogWarning("AuthController.Authenticate: Missing or invalid Authorization header");
                return Unauthorized();
            }

            var user = await _ablRepository.ApiUserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AuthController.Authenticate: User with ID {UserId} not found", userId);
                return Forbid();
            }
            if (user.Token != token)
            {
                _logger.LogWarning("AuthController.Authenticate: Invalid token for user {UserId}", userId);
                return Forbid();
            }

            var roles = user.Roles.Select(r => r.Role).ToList();
            var jwtToken = _jwtTokenService.CreateToken(userId, roles);

            _logger.LogInformation("AuthController.Authenticate: User {UserId} authenticated successfully", userId);
            return Ok(jwtToken.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthController.Authenticate: Error occurred while authenticating user {UserId}", userId);
            return StatusCode(500);
        }
    }

    private bool TryParseBasicAuthHeader(out Guid userId, out string token)
    {
        userId = Guid.Empty;
        token = string.Empty;

        var authorizationHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !AuthenticationHeaderValue.TryParse(authorizationHeader, out var headerValue)
            || !string.Equals(headerValue.Scheme, "Basic", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return false;
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(headerValue.Parameter));
        }
        catch (FormatException)
        {
            return false;
        }

        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex < 0)
        {
            return false;
        }

        var userIdPart = decoded[..separatorIndex];
        var tokenPart = decoded[(separatorIndex + 1)..];

        if (!Guid.TryParse(userIdPart, out userId))
        {
            return false;
        }

        token = tokenPart;
        return true;
    }
}
