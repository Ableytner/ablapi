using AblApi.Core.AppJwtToken.Dtos;

namespace AblApi.Core.AppJwtToken.Domain;

public class JwtToken
{
    public required string Token { get; set; }

    public required DateTime ExpiresAt { get; set; }

    public JwtTokenDto Map()
    {
        return new JwtTokenDto
        {
            Token = this.Token,
            ExpiresAt = this.ExpiresAt
        };
    }
}
