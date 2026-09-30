using AblApi.Common.Jobs;
using AblApi.Core.AppWillhaben;

namespace AblApi.Api;

// Used to manually debug locally
public class DebugBackgroundService(ILogger<DebugBackgroundService> logger, IServiceScopeFactory scopeFactory) : CyclicBackgroundService(logger)
{
    protected override string Name => nameof(DebugBackgroundService);
    protected override TimeSpan CycleTime => _fetchCycle;

    private readonly ILogger<DebugBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeSpan _fetchCycle = TimeSpan.FromSeconds(60);

    protected override async Task Initialize()
    {
        _logger.LogInformation("DebugBackgroundService started.");

        using var scope = _scopeFactory.CreateScope();
        var willhabenManager = scope.ServiceProvider.GetRequiredService<IWillhabenService>();

        var result = await willhabenManager.SearchAsync();
        _logger.LogInformation("Found {Count} results", result.Count);
        foreach (var item in result)
        {
            _logger.LogInformation("Id: {Id}, Heading: {Heading}, Description: {Description}, Price: {Price}, IsReserved: {IsReserved}, Km: {Km}, Url: {Url}", item.Id, item.Heading, item.Description, item.Price, item.IsReserved, item.Km, item.Url);
        }
    }

    protected override async Task Cyclic()
    {
        
    }
}
