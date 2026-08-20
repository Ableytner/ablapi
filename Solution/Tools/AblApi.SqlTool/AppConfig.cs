using AblApi.Core.AppSettings;

namespace AblApi.SqlTool;

public class AppConfig
{
    public required DatabaseAppSettings Database { get; set; }
}
