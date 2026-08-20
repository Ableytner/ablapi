namespace AblApi.DataAccess.Models.GTNH;

public class DailyVersion
{
    public long RunNumber { get; set; }

    public string Version { get; set; }

    public bool Success { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string RunUrl { get; set; }

    public string RunHtmlUrl { get; set; }

    public string? ClientDownloadUrl { get; set; }

    public string? ClientDownloadUrlJava8 { get; set; }

    public string? ServerDownloadUrl { get; set; }

    public string? ServerDownloadUrlJava8 { get; set; }
}
