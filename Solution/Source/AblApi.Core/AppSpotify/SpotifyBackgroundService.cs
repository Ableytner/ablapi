using AblApi.Common.Jobs;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppSpotify;

public class SpotifyBackgroundService(ILogger<SpotifyBackgroundService> logger, IServiceScopeFactory scopeFactory, SpotifyAppSettings appSettings) : CyclicBackgroundService(logger)
{
    protected override string Name => nameof(SpotifyBackgroundService);
    protected override TimeSpan CycleTime => _fetchCycle;

    private readonly ILogger<SpotifyBackgroundService> _logger = logger;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeSpan _fetchCycle = TimeSpan.FromSeconds(appSettings.FetchCycleInSeconds);

    protected override async Task Initialize()
    {
        _logger.LogInformation("SpotifyBackgroundService started.");
    }

    protected override async Task Cyclic()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAllPlaylistService>();
        var ablRepository = scope.ServiceProvider.GetRequiredService<IAblRepository>();

        var users = await ablRepository.SpotifyUserRepository.GetAllAsListAsync();

        foreach (var user in users)
        {
            // only update users with a playlist
            if (string.IsNullOrEmpty(user.AllPlaylistId))
            {
                continue;
            }

            try
            {
                await service.UpdateAsync(user.UserIdentifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update all_playlist for user {User}", user.UserIdentifier);
            }
        }
    }
}
