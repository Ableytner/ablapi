using AblApi.DataAccess.Models.GTNH;
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

    public DailyVersion ToDbo()
    {
        return new DailyVersion
        {
            Version = this.Version,
            RunNumber = this.RunNumber,
            Success = this.Success,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            RunUrl = this.RunUrl,
            RunHtmlUrl = this.RunUrlHtml,
            ClientDownloadUrl = this.DownloadUrls?.Client,
            ClientDownloadUrlJava8 = this.DownloadUrls?.ClientJava8,
            ServerDownloadUrl = this.DownloadUrls?.Server,
            ServerDownloadUrlJava8 = this.DownloadUrls?.ServerJava8
        };
    }

    public static DailyVersionDto FromDbo(DailyVersion version)
    {
        DownloadUrlsDto? downloadUrls = null;
        if (version.ClientDownloadUrl != null && version.ClientDownloadUrlJava8 != null && version.ServerDownloadUrl != null && version.ServerDownloadUrlJava8 != null)
        {
            downloadUrls = new DownloadUrlsDto
            {
                Client = version.ClientDownloadUrl,
                ClientJava8 = version.ClientDownloadUrlJava8,
                Server = version.ServerDownloadUrl,
                ServerJava8 = version.ServerDownloadUrlJava8
            };
        }

        return new DailyVersionDto
        {
            Version = version.Version,
            RunNumber = (int)version.RunNumber,
            Success = version.Success,
            CreatedAt = version.CreatedAt,
            UpdatedAt = version.UpdatedAt,
            RunUrl = version.RunUrl,
            RunUrlHtml = version.RunHtmlUrl,
            DownloadUrls = downloadUrls
        };
    }
}
