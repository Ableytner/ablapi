using System.Text.Json.Serialization;

namespace AblApi.Core.AppJwtToken.Dtos;

public class JwtTokenDto
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("expires_at")]
    public required DateTime ExpiresAt { get; set; }
}
