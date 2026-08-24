using AblApi.Common;

namespace AblApi.Api.Startup;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConfigProviders(this IConfigurationBuilder configuration)
    {
        DotEnv.LoadEnvVariables();

        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")?.ToLower();

        configuration.SetBasePath(Directory.GetCurrentDirectory());
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        if (!string.IsNullOrEmpty(environment))
        {
            configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
        }
        configuration.AddEnvironmentVariables();
        
        return configuration;
    }
}
