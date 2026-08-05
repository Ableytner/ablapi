using System.Text.Json.Serialization;

namespace AblApi.GTNH.Dtos;

public class DownloadUrlsDto
{
    [JsonPropertyName("client")]
    public required string Client { get; set; }

    [JsonPropertyName("server")]
    public required string Server { get; set; }
}
