using AblApi.Core.AppGithub;
using AblApi.DataAccess.Context;
using AblApi.GTNH;
using AblApi.Repositories;
using AblApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        services.AddTransient<IGithubService, GithubService>();
        services.AddTransient<IGTNHManager, GTNHManager>();

        services.AddHttpClient<GithubHttpClient>("Github");

        services.AddLocalization();

        services.AddScoped<IAblRepository, AblRepository>();
        services.AddDbContext<AblContext>(options =>
            options.UseSqlite(
                config.GetConnectionString("AblContext"),
                sqlite => sqlite.CommandTimeout(120))
        );

        return services;
    }
}
