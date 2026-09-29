using System.Text.Json.Serialization;

namespace AblApi.Core.AppNikoBot.Dtos;

public class InboxPayload
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("channel_id")]
    public required string ChannelId { get; set; }

    [JsonPropertyName("fields")]
    public InboxField[]? Fields { get; set; }
}

public class InboxField
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("value")]
    public required string Value { get; set; }

    [JsonPropertyName("inline")]
    public bool Inline { get; set; }
}
