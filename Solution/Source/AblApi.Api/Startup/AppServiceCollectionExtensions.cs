using AblApi.Common.Utilities;
using AblApi.Core.AppGithub;
using AblApi.Core.AppNikoBot;
using AblApi.Core.AppWillhaben;
using AblApi.GTNH;

namespace AblApi.Api.Startup;

internal static class AppServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        AddAppSettings(services, config);
        AddServices(services, config);
        AddBackgroundServices(services, config);

        services.AddMemoryCache();
        services.AddLocalization();

        return services;
    }

    private static void AddAppSettings(IServiceCollection services, IConfiguration config)
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
    }

    private static void AddServices(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IGithubHttpClient, GithubHttpClient>();
        services.AddTransient<IGithubService, GithubService>();

        services.AddTransient<IGTNewHorizonsService, GTNewHorizonsService>();
        services.AddTransient<IGTNHService, GTNHService>();

        services.AddTransient<INikoBotService, NikoBotService>();

        services.AddSingleton<IWillhabenHttpClient, WillhabenHttpClient>();
        services.AddTransient<IWillhabenService, WillhabenService>();
    }

    private static void AddBackgroundServices(IServiceCollection services, IConfiguration config)
    {
        services.AddHostedService<GTNHBackgroundService>();
        services.AddHostedService<WillhabenBackgroundService>();

        if (EnvironmentHelper.IsDevelopment())
        {
            services.AddHostedService<DebugBackgroundService>();
        }
    }
}
