namespace AblApi.Core.AppJwtToken.Domain;

public class JwtToken
{
    public required string Token { get; set; }

    public required DateTime ExpiresAt { get; set; }
}
