using AblApi.Common.Jobs;
using AblApi.Core.AppNikoBot;
using AblApi.Core.AppNikoBot.Domain;
using AblApi.Core.AppNikoBot.Enum;
using AblApi.Core.AppWillhaben.Domain;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppWillhaben;

public class WillhabenBackgroundService(ILogger<WillhabenBackgroundService> logger, IServiceScopeFactory scopeFactory) : CyclicBackgroundService(logger)
{
    protected override string Name => nameof(WillhabenBackgroundService);
    protected override TimeSpan CycleTime => TimeSpan.FromMinutes(1);

    private readonly ILogger<WillhabenBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task Initialize()
    {
        _logger.LogInformation("WillhabenBackgroundService started.");
    }

    protected override async Task Cyclic()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IWillhabenService>();
        var ablRepository = scope.ServiceProvider.GetRequiredService<IAblRepository>();
        var nikobotService = scope.ServiceProvider.GetRequiredService<INikoBotService>();

        var configs = await service.ListConfigsAsync();
        var newResults = new HashSet<WillhabenListing>();

        foreach (var config in configs)
        {
            var listings = await service.SearchAsync(config);

            foreach (var listing in listings)
            {
                if (!await service.HasSeenListingAsync(listing))
                {
                    await service.AddSeenListingAsync(listing);
                    newResults.Add(listing);
                }
            }
        }

        if (newResults.Count == 0)
        {
            _logger.LogDebug("Found no new listings");
            return;
        }

        _logger.LogInformation("Found {Count} new listings, sending notifications", newResults.Count);

        foreach (var result in newResults)
        {
            var message = new NikoBotEmbedMessage()
            {
                Title = "Found new Willhaben listing",
                Message = "",
                Color = NikoBotColor.Green,
                ChannelId = "owner",
                Fields = [
                    new NikoBotEmbedField()
                    {
                        Name = "Title",
                        Value = result.Heading
                    },
                    new NikoBotEmbedField()
                    {
                        Name = "Location",
                        Value = $"{result.Postcode} {result.State}"
                    },
                    new NikoBotEmbedField()
                    {
                        Name = "Price",
                        Value = result.Price?.ToString() ?? "unknown"
                    },
                    new NikoBotEmbedField()
                    {
                        Name = "Url",
                        Value = result.Url
                    }
                ]
            };

            var success = await nikobotService.SendEmbedAsync(message);
            if (!success)
            {
                _logger.LogError("Failed to send NikoBot message for listing {ListingUrl}", result.Url);
            }
        }
    }
}
