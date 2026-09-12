using AblApi.Common.Jobs;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AblApi.GTNH;

public class GTNHBackgroundService(ILogger<GTNHBackgroundService> logger, IServiceScopeFactory scopeFactory, GTNHAppSettings gtnhConfiguration) : CyclicBackgroundService(logger)
{
    protected override string Name => nameof(GTNHBackgroundService);
    protected override TimeSpan CycleTime => _fetchCycle;

    private readonly ILogger<GTNHBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeSpan _fetchCycle = TimeSpan.FromSeconds(gtnhConfiguration.FetchCycleInSeconds);

    protected override async Task Initialize()
    {
        using var scope = _scopeFactory.CreateScope();
        var gtnhManager = scope.ServiceProvider.GetRequiredService<IGTNHService>();

        if (!await gtnhManager.TestToken())
        {
            throw new InvalidOperationException("Github token validation failed.");
        }

        _logger.LogInformation("GTNHBackgroundService started.");

        await BackfillDailyVersions();
    }

    protected override async Task Cyclic()
    {
        using var scope = _scopeFactory.CreateScope();
        var gtnhManager = scope.ServiceProvider.GetRequiredService<IGTNHService>();
        var ablRepository = scope.ServiceProvider.GetRequiredService<IAblRepository>();

        var latestDailyVersion = await gtnhManager.GetLatestDailyVersionRunNumberAsync();
        var latestStoredDailyVersion = await ablRepository.GTNHDailyVersionRepository.GetLatestAsync();

        if (latestStoredDailyVersion == null || latestDailyVersion > latestStoredDailyVersion.RunNumber)
        {
            var dailyVersion = await gtnhManager.GetLatestDailyVersionAsync();

            ablRepository.GTNHDailyVersionRepository.Add(dailyVersion.ToDbo());

            await ablRepository.SaveChangesAsync();
            _logger.LogInformation("Saved new GTNH daily version {dailyVersion}", dailyVersion.RunNumber);
        }
    }

    private async Task BackfillDailyVersions()
    {
        // TODO: fetch and store all older daily versions not yet in the database
    }
}
