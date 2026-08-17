using AblApi.Common.Jobs;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace AblApi.GTNH;

internal class GTNHBackgroundService(ILogger<GTNHBackgroundService> logger, IGTNHManager gtnhManager, IAblRepository ablRepository, GTNHAppSettings gtnhConfiguration) : CyclicBackgroundService(logger)
{
    protected override string Name => nameof(GTNHBackgroundService);
    protected override TimeSpan CycleTime => _fetchCycle;

    private readonly ILogger<GTNHBackgroundService> _logger = logger;
    private readonly IGTNHManager _gtnhManager = gtnhManager;
    private readonly IAblRepository _ablRepository = ablRepository;
    private readonly TimeSpan _fetchCycle = TimeSpan.FromSeconds(gtnhConfiguration.FetchCycleInSeconds);

    protected override async Task Initialize()
    {
        if (!await _gtnhManager.TestToken())
        {
            throw new InvalidOperationException("Github token validation failed.");
        }

        await BackfillDailyVersions();
    }

    protected override async Task Cyclic()
    {
        var latestDailyVersion = await _gtnhManager.GetLatestDailyVersionRunNumberAsync();
        var latestStoredDailyVersion = await _ablRepository.GTNHDailyVersionRepository.GetLatestDailyVersionAsync();

        if (latestDailyVersion > latestStoredDailyVersion.RunNumber)
        {
            var dailyVersion = await _gtnhManager.GetSpecificDailyVersionAsync(latestDailyVersion);
            // TODO: store the new daily version
        }
    }

    private async Task BackfillDailyVersions()
    {
        // TODO: fetch and store all older daily versions not yet in the database
    }
}
