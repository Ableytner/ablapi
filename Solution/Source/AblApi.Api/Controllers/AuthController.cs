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

    [HttpGet("{userId:guid}/")]
    public async Task<ActionResult<JwtTokenDto>> Authenticate(Guid userId)
    {
        try
        {
            var user = await _ablRepository.ApiUserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("AuthController.Authenticate: User with ID {UserId} not found", userId);
                return Forbid();
            }

            var roles = user.Roles.Select(r => r.Role).ToList();
            var token = _jwtTokenService.CreateToken(userId, roles);

            _logger.LogInformation("AuthController.Authenticate: User {UserId} authenticated successfully", userId);
            return Ok(token.Map());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthController.Authenticate: Error occurred while authenticating user {UserId}", userId);
            return StatusCode(500);
        }
    }
}
