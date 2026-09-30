using Serilog.Events;

namespace AblApi.Api.Logging;

public static class LogUtil
{
    public static LogEventLevel GetLogLevel(string logLevel)
    {
        return logLevel switch
        {
            "Verbose" => LogEventLevel.Verbose,
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    public static string GetLogLevelString(LogEventLevel logLevel)
    {
        return logLevel switch
        {
            LogEventLevel.Verbose => "Verbose",
            LogEventLevel.Debug => "Debug",
            LogEventLevel.Information => "Information",
            LogEventLevel.Warning => "Warning",
            LogEventLevel.Error => "Error",
            LogEventLevel.Fatal => "Fatal",
            _ => "Information"
        };
    }
}