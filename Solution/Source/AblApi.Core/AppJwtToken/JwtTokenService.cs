using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken.Domain;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace AblApi.Core.AppJwtToken;

public class JwtTokenService(JwtAppSettings jwtSettings) : IJwtTokenService
{
    private readonly JwtAppSettings _jwtSettings = jwtSettings;

    public JwtToken CreateToken(Guid userId, IEnumerable<ApiAccessRole> roles)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            // which user the token is for
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            // when the token was issued
            new(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)now).ToUnixTimeSeconds().ToString()),
            // unique token identifier
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // one claim per role
        claims.AddRange(roles.Select(role => new Claim("role", role.ToString())));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            Expires = expiresAt,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);

        return new JwtToken {
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}
