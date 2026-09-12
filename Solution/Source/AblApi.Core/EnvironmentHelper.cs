using AblApi.Common;

namespace AblApi.Core;

public static class EnvironmentHelper
{
    private static string? _environment;

    public static bool IsDevelopment()
    {
        return GetEnvironment().StartsWith("dev", StringComparison.CurrentCultureIgnoreCase)
               || GetEnvironment().StartsWith("local", StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool IsProduction()
    {
        return GetEnvironment().StartsWith("prod", StringComparison.CurrentCultureIgnoreCase);
    }

    public static string GetEnvironment()
    {
        if (_environment is null)
        {
            DotEnv.LoadEnvVariables();

            _environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";
        }

        return _environment;
    }
}
