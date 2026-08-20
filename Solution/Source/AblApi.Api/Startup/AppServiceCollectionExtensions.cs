using AblApi.Core.AppGithub;
using AblApi.GTNH;

namespace AblApi.Api.Startup;

internal static class AppServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        var githubConfig = new GithubAppSettings();
        config.GetSection(GithubAppSettings.SectionName).Bind(githubConfig);
        services.AddSingleton(githubConfig);

        var gtnhConfig = new GTNHAppSettings();
        config.GetSection(GTNHAppSettings.SectionName).Bind(gtnhConfig);
        services.AddSingleton(gtnhConfig);

        services.AddSingleton<IGithubHttpClient, GithubHttpClient>();
        services.AddTransient<IGithubService, GithubService>();
        services.AddTransient<IGTNHService, GTNHService>();

        services.AddLocalization();

        return services;
    }
}
