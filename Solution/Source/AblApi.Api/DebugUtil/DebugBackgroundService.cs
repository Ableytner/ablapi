using AblApi.Common.Extensions;
using AblApi.Common.Jobs;
using AblApi.Core.AppLogging.Dtos;
using AblApi.Core.AppWillhaben;
using AblApi.Core.AppWillhaben.Dtos;

namespace AblApi.Api.DebugUtil;

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
        var willhabenService = scope.ServiceProvider.GetRequiredService<IWillhabenService>();

        var config = new WillhabenConfigDto
        {
            Keyword = "9060 XT",
            Category = 5882,
            Rows = 100,
            PriceMin = 150,
            PriceMax = 500,
            FilterPaylivery = false,
            HandoverTypes = [],
            AllowedStates = [
                "Steiermark", "Kärnten", "Niederösterreich", "Burgenland", "Oberösterreich", "Wien"
            ],
            MustInclude = [
                "9060", "16"
            ],
            MustExclude = [
                "8GB", "8 GB", "8gb"
            ]
        };

        await willhabenService.CreateConfigAsync(config);
    }

    protected override async Task Cyclic()
    {
        
    }

    private async Task SearchAndPrintWillhabenConfig(IWillhabenService service, WillhabenConfigDto config)
    {
        var result = await service.SearchAsync(config);
        _logger.LogInformation("Found {Count} results", result.Count);
        foreach (var item in result)
        {
            _logger.LogInformation("Id: {Id}, Heading: {Heading}, Description: {Description}, Price: {Price}, IsReserved: {IsReserved}, Km: {Km}, Url: {Url}", item.Id, item.Heading, item.Description, item.Price, item.IsReserved, item.Km, item.Url);
        }

        using (_logger.BeginScope(new Dictionary<string, object> { { "Sender", "DebugService" } }))
        {
            _logger.LogError("This would go to {Service}!", "NikoBot");
        }
    }

    private async Task TestLogController()
    {
        var httpClient = new DebugHttpClient();

        var logMessageDto = new LogMessageDto()
        {
            LogLevel = "Error",
            Sender = "DebugBackgroundService",
            Message = "This message was sent to the LogController"
        };
        var request = JsonContent.Create(logMessageDto);
        _logger.LogInformation(request.ReadAsString());

        var response = await httpClient.PostAsync("log", request);
        _logger.LogInformation(response.Content.ReadAsString());
    }
}
