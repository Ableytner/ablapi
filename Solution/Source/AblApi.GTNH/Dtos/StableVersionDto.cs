using AblApi.DataAccess.Models.GTNH;
using System.Text.Json.Serialization;

namespace AblApi.GTNH.Dtos;

public class StableVersionDto
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }

    [JsonPropertyName("downloads")]
    public required DownloadUrlsDto DownloadUrls { get; set; }

    public StableVersion ToDbo()
    {
        return new StableVersion
        {
            Version = this.Version,
            CreatedAt = this.CreatedAt,
            ClientDownloadUrl = this.DownloadUrls.Client,
            ClientDownloadUrlJava8 = this.DownloadUrls.ClientJava8,
            ServerDownloadUrl = this.DownloadUrls.Server,
            ServerDownloadUrlJava8 = this.DownloadUrls.ServerJava8
        };
    }
}
