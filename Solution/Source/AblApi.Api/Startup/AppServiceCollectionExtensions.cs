using AblApi.Core.AppGithubApi;
using AblApi.GTNH;

namespace AblApi.Api.Startup;

internal static class AppServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
    {
        var gtnhConfig = new GTNHAppSettings();
        config.GetSection(GTNHAppSettings.SectionName).Bind(gtnhConfig);
        services.AddSingleton(gtnhConfig);

        services.AddTransient<IGithubApiService, GithubApiService>();
        services.AddTransient<IGTNHManager, GTNHManager>();

        services.AddHttpClient<GithubHttpClient>("GithubApi");

        services.AddLocalization();

        return services;
    }
}