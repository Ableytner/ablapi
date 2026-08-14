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
    protected readonly AblContext _ablContext;

    protected readonly IConfiguration Configuration;

    public ServiceProvider ServiceProvider { get; }

    protected BaseFixture(AblContext ablContext)
    {
        _ablContext = ablContext;

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
            .WriteTo.File("/var/log/EMSBackend/logs-ablcontext-{Date}.txt", rollingInterval: RollingInterval.Day)
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
        services.AddTransient<IGithubService, GithubService>();
        services.AddTransient<IGTNHManager, GTNHManager>();

        services.AddHttpClient<GithubHttpClient>("Github");
    }

    private void ConfigureDatabase(IServiceCollection services)
    {
        services.AddScoped<IAblRepository, AblRepository>();

        services.AddDbContext<AblContext>(_ => GetDbContext());
    }

    private AblContext GetDbContext()
    {
        return _ablContext;
    }
}
