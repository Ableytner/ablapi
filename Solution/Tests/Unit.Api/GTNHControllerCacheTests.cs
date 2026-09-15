using AblApi.Api.Controllers;
using AblApi.GTNH;
using AblApi.GTNH.Dtos;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace Tests.Unit.Api;

public class GTNHControllerCacheTests
{
    private static DownloadUrlsDto CreateDownloadUrls() => new()
    {
        Client = "https://example.com/client",
        ClientJava8 = "https://example.com/client-java8",
        Server = "https://example.com/server",
        ServerJava8 = "https://example.com/server-java8"
    };

    [Fact]
    public async Task GetLatestDailyVersion_CallsServiceOnlyOnce()
    {
        // Arrange
        var gtnhService = Substitute.For<IGTNHService>();
        var cache = Substitute.For<IMemoryCache>();
        var settings = new GTNHAppSettings { CacheExpirationMinutes = 5 };
        var controller = new GTNHController(gtnhService, cache, settings);

        var cachedDto = new DailyVersionDto
        {
            Version = "1.0",
            RunNumber = 42,
            Success = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RunUrl = "https://example.com/run",
            RunUrlHtml = "https://example.com/run-html",
            DownloadUrls = CreateDownloadUrls()
        };
        cache.TryGetValue(Arg.Any<string>(), out Arg.Any<DailyVersionDto?>())
            .ReturnsForAnyArgs(call => { call[1] = cachedDto; return true; });

        // Act
        await controller.GetLatestDailyVersion();

        // Assert
        await gtnhService.DidNotReceive().GetLatestDailyVersionAsync(null);
    }

    [Fact]
    public async Task GetLatestStableVersion_CallsServiceOnlyOnce()
    {
        // Arrange
        var gtnhService = Substitute.For<IGTNHService>();
        var cache = Substitute.For<IMemoryCache>();
        var settings = new GTNHAppSettings { CacheExpirationMinutes = 5 };
        var controller = new GTNHController(gtnhService, cache, settings);

        var cachedDto = new StableVersionDto
        {
            Version = "1.2.3",
            CreatedAt = DateTime.UtcNow,
            DownloadUrls = CreateDownloadUrls()
        };
        cache.TryGetValue(Arg.Any<string>(), out Arg.Any<StableVersionDto?>())
            .ReturnsForAnyArgs(call => { call[1] = cachedDto; return true; });

        // Act
        await controller.GetLatestStableVersion();

        // Assert
        await gtnhService.DidNotReceive().GetLatestStableVersionAsync();
    }
}
