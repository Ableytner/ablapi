using AblApi.Common;

namespace AblApi.Api.Startup;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConfigProviders(this IConfigurationBuilder configuration)
    {
        DotEnv.LoadEnvVariables();

        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        configuration.SetBasePath(Directory.GetCurrentDirectory());
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        if (!string.IsNullOrEmpty(environment))
        {
            configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false);
        }
        configuration.AddEnvironmentVariables();
        
        return configuration;
    }
}
