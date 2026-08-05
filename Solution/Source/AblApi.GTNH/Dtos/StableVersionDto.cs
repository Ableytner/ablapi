using System.Text.Json.Serialization;

namespace AblApi.GTNH.Dtos;

public class StableVersionDto
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("downloads")]
    public required DownloadUrlsDto DownloadUrls { get; set; }
}
