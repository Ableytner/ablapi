using System.Text.Json.Serialization;

namespace AblApi.Core.AppNikoBot.Dtos;

public class InboxResponse
{
    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}
