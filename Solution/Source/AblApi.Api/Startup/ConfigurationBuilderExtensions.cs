using AblApi.Common;
using AblApi.Core;

namespace AblApi.Api.Startup;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddConfigProviders(this IConfigurationBuilder configuration)
    {
        DotEnv.LoadEnvVariables();

        configuration.SetBasePath(Directory.GetCurrentDirectory());
        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        configuration.AddJsonFile($"appsettings.{EnvironmentHelper.GetEnvironment()}.json", optional: true, reloadOnChange: false);
        configuration.AddEnvironmentVariables();
        
        return configuration;
    }
}
