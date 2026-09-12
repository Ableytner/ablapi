namespace AblApi.DataAccess.Models.GTNH;

public class StableVersion
{
    // PK
    public int Id { get; set; }

    public string Version { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ClientDownloadUrl { get; set; }

    public string ClientDownloadUrlJava8 { get; set; }

    public string ServerDownloadUrl { get; set; }

    public string ServerDownloadUrlJava8 { get; set; }
}
