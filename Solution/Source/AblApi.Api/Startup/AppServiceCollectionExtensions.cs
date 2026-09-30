using AblApi.Core.AppGithub;
using AblApi.Core.AppNikoBot;
using AblApi.Core.AppWillhaben;
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

        var nikobotConfig = new NikoBotAppSettings();
        config.GetSection(NikoBotAppSettings.SectionName).Bind(nikobotConfig);
        if (string.IsNullOrEmpty(nikobotConfig.BaseUrl) || string.IsNullOrEmpty(nikobotConfig.ApiSecret))
        {
            throw new InvalidOperationException("NikoBot baseurl is not configured.");
        }
        services.AddSingleton(nikobotConfig);

        var willhabenConfig = new WillhabenAppSettings();
        config.GetSection(WillhabenAppSettings.SectionName).Bind(willhabenConfig);
        services.AddSingleton(willhabenConfig);

        services.AddHostedService<DebugBackgroundService>();
        services.AddSingleton<IGithubHttpClient, GithubHttpClient>();
        services.AddTransient<IGithubService, GithubService>();
        services.AddTransient<IGTNewHorizonsService, GTNewHorizonsService>();
        services.AddTransient<IGTNHService, GTNHService>();
        services.AddTransient<INikoBotService, NikoBotService>();
        //services.AddHostedService<GTNHBackgroundService>();
        services.AddSingleton<IWillhabenHttpClient, WillhabenHttpClient>();
        services.AddTransient<IWillhabenService, WillhabenService>();

        services.AddMemoryCache();
        services.AddLocalization();

        return services;
    }
}
