using AblApi.GTNH.Dtos;
using Integration.Api.Fixture;
using System.Net.Http.Json;

namespace Integration.Api;

public class GTNHControllerTests : TestBase
{
    private const string BaseUrl = "api/gtnh";

    [Fact]
    public async Task GetDailyVersion_ReturnsValidJson()
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
}
