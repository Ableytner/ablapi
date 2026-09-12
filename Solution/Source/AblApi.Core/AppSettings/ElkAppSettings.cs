namespace AblApi.Core.AppSettings;

public class ElkAppSettings
{
    public const string SectionName = "ElkConfig";

    public string ElkUrl { get; set; }

    public string ApiKey { get; set; }

    public string LogLevel { get; set; }

    public string DataStreamType { get; set; }

    public string DataStreamDataSet { get; set; }

    public string DataStreamNamespace { get; set; }
}
