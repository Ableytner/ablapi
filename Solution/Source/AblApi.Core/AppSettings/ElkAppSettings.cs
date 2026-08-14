namespace AblApi.Core.AppSettings;

public class ElkAppSettings
{
    public required string ElkUrl { get; set; }

    public required string ApiKey { get; set; }

    public string LogLevel { get; set; } = "Information";

    public string DataStreamType { get; set; } = "abl";

    public string DataStreamDataSet { get; set; } = "AblApi";

    public string DataStreamNamespace { get; set; } = "0001";
}
