using AblApi.DataAccess.Models.GTNH;
using System.Text.Json.Serialization;

namespace AblApi.GTNH.Dtos;

public class StableVersionDto
{
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    [JsonPropertyName("downloads")]
    public required DownloadUrlsDto DownloadUrls { get; set; }

    public StableVersion Map()
    {
        return new StableVersion
        {
            Version = this.Version,
            CreatedAt = DateTime.UtcNow,
            ClientDownloadUrl = this.DownloadUrls.Client,
            ClientDownloadUrlJava8 = this.DownloadUrls.ClientJava8,
            ServerDownloadUrl = this.DownloadUrls.Server,
            ServerDownloadUrlJava8 = this.DownloadUrls.ServerJava8
        };
    }
}
