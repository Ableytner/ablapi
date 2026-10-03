using System.Text.Json.Serialization;

namespace AblApi.Core.AppLogging.Dtos;

public class LogMessageDto
{
    [JsonPropertyName("log_level")]
    public required string LogLevel { get; set; }
    
    [JsonPropertyName("sender")]
    public required string Sender { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
