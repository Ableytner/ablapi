using AblApi.GTNH;
using Integration.Api.Fixture;

namespace Integration.Api;

public class GTNewHorizonsServiceTests : TestBase
{
    [Fact]
    public async Task GetLatestStableVersionAsync_ReturnsHighestVersion()
    {
        // Arrange
        using var server = new MockHttpServer(GetVersionsJson());
        var appSettings = new GTNHAppSettings { StableVersionApiUrl = server.Url };
        var service = new GTNewHorizonsService(appSettings);

        // Act
        var result = await service.GetLatestStableVersionAsync(CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2.9.0-beta-2", result.Version);
    }

    [Fact]
    public async Task GetSpecificStableVersionAsync_ExistingVersion_ReturnsMatchingVersion()
    {
        // Arrange
        using var server = new MockHttpServer(GetVersionsJson());
        var appSettings = new GTNHAppSettings { StableVersionApiUrl = server.Url };
        var service = new GTNewHorizonsService(appSettings);

        // Act
        var result = await service.GetSpecificStableVersionAsync("2.7.4", CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2.7.4", result.Version);
    }

    [Fact]
    public async Task GetSpecificStableVersionAsync_MissingVersion_ThrowsInvalidOperationException()
    {
        // Arrange
        using var server = new MockHttpServer(GetVersionsJson());
        var appSettings = new GTNHAppSettings { StableVersionApiUrl = server.Url };
        var service = new GTNewHorizonsService(appSettings);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetSpecificStableVersionAsync("9.9.9", CancellationToken));
    }

    [Fact]
    public async Task GetLatestStableVersionAsync_RealApiCall_ParsesCorrectly()
    {
        // Arrange
        var appSettings = new GTNHAppSettings { StableVersionApiUrl = "https://www.gtnewhorizons.com/versions.json" };
        var service = new GTNewHorizonsService(appSettings);

        // Act
        var result = await service.GetLatestStableVersionAsync(CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Version));
    }

    private static string GetVersionsJson()
    {
        return File.ReadAllText("Data/gtnh_versions.json");
    }
}
