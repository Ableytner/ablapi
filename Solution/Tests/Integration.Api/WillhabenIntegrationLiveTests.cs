using System.Text.Json;
using AblApi.Core.AppWillhaben;

namespace Integration.Api;

public class WillhabenIntegrationLiveTests
{
//    [Fact]
    [Fact(Skip = "Live API test")]
    public async Task GetListingsAsync_ParsesRealHtmlAndJson()
    {
        // Arrange
        var settings = new WillhabenAppSettings
        {
            Category = 5882,
            Rows = 5,
        };

        var httpClient = new WillhabenHttpClient(settings);

        // Act
        var searchUrl = httpClient.BuildSearchUrl();
        var dtos = await httpClient.GetListingsAsync(searchUrl, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(dtos);

        foreach (var dto in dtos)
        {
            Console.WriteLine($"=== Listing ===");
            Console.WriteLine($"  Id:        {dto.Id}");
            Console.WriteLine($"  AdId:      {dto.AdId}");
            Console.WriteLine($"  Heading:   {dto.Heading}");
            Console.WriteLine($"  Price:     {dto.Price}");
            Console.WriteLine($"  Location:  {dto.Location}");
            Console.WriteLine($"  State:     {dto.State}");
            Console.WriteLine($"  SeoUrl:    {dto.SeoUrl}");
            Console.WriteLine($"  Coordinates: {dto.Coordinates}");
            Console.WriteLine($"  AdvertStatus: {dto.AdvertStatus?.Description}");
            Console.WriteLine();
        }

        // Basic sanity checks
        Assert.All(dtos, dto =>
        {
            Assert.NotNull(dto.Id);
            Assert.NotNull(dto.Heading);
            Assert.NotNull(dto.SeoUrl);
        });
    }

//    [Fact]
    [Fact(Skip = "Live API test")]
    public async Task GetListingsAsync_RawJsonOutput()
    {
        // Arrange
        var settings = new WillhabenAppSettings
        {
            Category = 5882,
            Rows = 3,
        };

        var httpClient = new WillhabenHttpClient(settings);

        // Act
        var searchUrl = httpClient.BuildSearchUrl();
        var response = await httpClient.GetAsync(searchUrl, TestContext.Current.CancellationToken);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Extract and print the __NEXT_DATA__ json
        var scriptTag = "<script id=\"__NEXT_DATA__\" type=\"application/json\">";
        var startIndex = html.IndexOf(scriptTag, StringComparison.Ordinal);
        if (startIndex < 0)
        {
            Assert.Fail("Could not find __NEXT_DATA__ script tag in HTML");
            return;
        }

        startIndex += scriptTag.Length;
        var endIndex = html.IndexOf("</script>", startIndex, StringComparison.Ordinal);
        var jsonContent = html[startIndex..endIndex];

        // Print the raw json for inspection
        Console.WriteLine(jsonContent);

        // Verify it's valid json
        using var doc = JsonDocument.Parse(jsonContent);
        Assert.True(doc.RootElement.TryGetProperty("props", out _));
    }
}
