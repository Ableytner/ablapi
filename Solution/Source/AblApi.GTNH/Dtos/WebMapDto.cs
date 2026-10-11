using System.Text.Json.Serialization;

namespace AblApi.GTNH.Dtos;

public class WebMapDto
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("download_url")]
    public required string DownloadUrl { get; set; }
}
