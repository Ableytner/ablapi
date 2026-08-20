using AblApi.Core.AppGithub;
using AblApi.Core.AppJwtToken;
using AblApi.DataAccess.Context;
using AblApi.GTNH;
using AblApi.Repositories;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Tests.Unit.Api.Mocks;

public abstract class BaseFixture
{
    protected readonly IConfiguration Configuration;

    public AblContext AblContext { get; }

    public ServiceProvider ServiceProvider { get; }

    protected BaseFixture(AblContext ablContext)
    {
        AblContext = ablContext;

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();

        var services = new ServiceCollection();

        ConfigureApps(services);
        ConfigureDatabase(services);

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureApps(IServiceCollection services)
    {
        var fileLogger = new LoggerConfiguration()
            .WriteTo.File("/var/log/AblApi/logs-ablcontext-{Date}.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(fileLogger);
        });

        services.AddSingleton(Configuration);

        var githubConfig = new GithubAppSettings();
        Configuration.GetSection(GithubAppSettings.SectionName).Bind(githubConfig);
        services.AddSingleton(githubConfig);

        var gtnhConfig = new GTNHAppSettings();
        Configuration.GetSection(GTNHAppSettings.SectionName).Bind(gtnhConfig);
        services.AddSingleton(gtnhConfig);

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IGithubHttpClient, GithubHttpClient>();
        services.AddTransient<IGithubService, GithubService>();
        services.AddTransient<IGTNHService, GTNHService>();
    }

    private void ConfigureDatabase(IServiceCollection services)
    {
        services.AddScoped<IAblRepository, AblRepository>();

        services.AddSingleton(AblContext);
    }
}
