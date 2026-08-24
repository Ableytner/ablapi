using AblApi.GTNH.Dtos;
using Integration.Api.Fixture;
using Integration.Api.GTNH.Fixture;
using System.Net.Http.Json;

namespace Integration.Api.GTNH;

public class GTNHControllerTests : GTNHTestBase
{
    private const string BaseUrl = "api/gtnh";

    private static readonly MockHttpServer StableVersionServer = new(GetVersionsJson());

    public GTNHControllerTests() : base(StableVersionServer.Url)
    {
    }

    [Fact]
    public async Task GetSpecificDailyVersion_ReturnsValidJson()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/daily/666");

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var dailyVersion = await response.Content.ReadFromJsonAsync<DailyVersionDto>(CancellationToken);
        Assert.NotNull(dailyVersion);
        Assert.Equal(666, dailyVersion.RunNumber);
    }

    [Fact]
    public async Task GetLatestStableVersion_ReturnsValidJson()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/stable/latest");

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var stableVersion = await response.Content.ReadFromJsonAsync<StableVersionDto>(CancellationToken);
        Assert.NotNull(stableVersion);
    }

    [Fact]
    public async Task GetSpecificStableVersion_ReturnsValidJson()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/stable/2.8.1");

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var stableVersion = await response.Content.ReadFromJsonAsync<StableVersionDto>(CancellationToken);
        Assert.NotNull(stableVersion);
        Assert.Equal("2.8.1", stableVersion.Version);
    }

    private static string GetVersionsJson()
    {
        return File.ReadAllText("GTNH/Data/versions.json");
    }
}
