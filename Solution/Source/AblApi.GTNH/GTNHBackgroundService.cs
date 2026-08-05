using AblApi.Common.Jobs;
using Microsoft.Extensions.Logging;

namespace AblApi.GTNH;

internal class GTNHBackgroundService(ILogger<GTNHBackgroundService> logger, GTNHAppSettings gtnhConfiguration) : CyclicBackgroundService(logger)
{
    protected override string Name => nameof(GTNHBackgroundService);
    protected override TimeSpan CycleTime => _fetchCycle;

    private readonly ILogger<GTNHBackgroundService> _logger = logger;
    private readonly TimeSpan _fetchCycle = TimeSpan.FromSeconds(gtnhConfiguration.FetchCycleInSeconds);

    protected override Task Initialize()
    {
        // TODO: check if Github token is valid

        return Task.CompletedTask;
    }

    protected override async Task Cyclic()
    {
        // TODO: fetch and store GTNH data
    }
}
