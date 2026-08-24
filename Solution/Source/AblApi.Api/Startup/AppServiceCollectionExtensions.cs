using AblApi.Core.AppGithub;
using AblApi.GTNH;

namespace AblApi.Api.Startup;

internal static class AppServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        var githubConfig = new GithubAppSettings();
        config.GetSection(GithubAppSettings.SectionName).Bind(githubConfig);
        if (string.IsNullOrEmpty(githubConfig.Token) || githubConfig.Token == "GITHUBTOKEN")
        {
            throw new InvalidOperationException("Github token is not configured.");
        }
        services.AddSingleton(githubConfig);

        var gtnhConfig = new GTNHAppSettings();
        config.GetSection(GTNHAppSettings.SectionName).Bind(gtnhConfig);
        services.AddSingleton(gtnhConfig);

        services.AddSingleton<IGithubHttpClient, GithubHttpClient>();
        services.AddTransient<IGithubService, GithubService>();
        services.AddTransient<IGTNewHorizonsService, GTNewHorizonsService>();
        services.AddTransient<IGTNHService, GTNHService>();
        services.AddHostedService<GTNHBackgroundService>();

        services.AddLocalization();

        return services;
    }
}
