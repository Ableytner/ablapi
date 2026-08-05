using System.Text.Json.Serialization;

namespace AblApi.GTNH.Dtos;

public class DailyVersionDto
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("run_number")]
    public required int RunNumber { get; set; }

    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required DateTime UpdatedAt { get; set; }

    [JsonPropertyName("run_url")]
    public required string RunUrl { get; set; }

    [JsonPropertyName("run_url_html")]
    public required string RunUrlHtml { get; set; }

    [JsonPropertyName("downloads")]
    public required DownloadUrlsDto? DownloadUrls { get; set; }
}
