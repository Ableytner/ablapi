namespace AblApi.Core.AppSettings;

public class CorsAppSettings
{
    public const string SectionName = "Cors";

    public string AllowedOrigins { get; set; } = string.Empty;

    public string[] AllowedOriginsArray => AllowedOrigins
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
