namespace AblApi.Core.AppJwtToken.Dtos;

public class JwtTokenDto
{
    public required string Token { get; set; }

    public required DateTime ExpiresAt { get; set; }
}
