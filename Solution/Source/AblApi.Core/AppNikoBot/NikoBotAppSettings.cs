namespace AblApi.Core.AppNikoBot;

public class NikoBotAppSettings
{
    public const string SectionName = "NikoBotConfig";

    public string BaseUrl { get; set; }

    public string ApiSecret { get; set; }
}
