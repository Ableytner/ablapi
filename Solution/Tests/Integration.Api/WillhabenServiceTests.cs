using AblApi.Core.AppSettings;
using AblApi.Core.AppWillhaben;
using AblApi.Core.AppWillhaben.Dtos;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Integration.Api;

public class WillhabenServiceTests : TestBase
{
    private readonly ILogger<WillhabenService> _logger;
    private readonly WillhabenAppSettings _config;

    public WillhabenServiceTests()
    {
        _logger = Substitute.For<ILogger<WillhabenService>>();

        _config = new WillhabenAppSettings
        {
            Keyword = "test gpu",
            Category = 5882,
            Rows = 100,
            PriceMin = 100,
            PriceMax = 500,
            AllowedStates = ["Wien", "Niederösterreich"],
            MustInclude = ["rtx"],
            MustExclude = ["gtx"],
            KmMax = 50000,
            SortByDistance = true,
            ReferenceLat = 48.2082,
            ReferenceLon = 16.3738,
            MaxDistanceKm = 50,
        };
    }

    [Fact]
    public async Task SearchAsync_WithMockData_ReturnsFilteredListings()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient();
        var service = new WillhabenService(_logger, _config, mockHttpClient);

        // Act
        var result = await service.SearchAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task SearchAsync_PriceFilter_RemovesOutOfRangeListings()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient();
        var config = new WillhabenAppSettings
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            PriceMin = 200,
            PriceMax = 300,
        };
        var service = new WillhabenService(_logger, config, mockHttpClient);

        // Act
        var result = await service.SearchAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result, listing =>
        {
            Assert.NotNull(listing.Price);
            var price = (int)listing.Price;
            Assert.True(price >= 200 && price <= 300, $"Price {price} is out of range [200, 300]");
        });
    }

    [Fact]
    public async Task SearchAsync_KeywordFilter_RemovesNonMatchingListings()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient();
        var config = new WillhabenAppSettings
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            MustInclude = ["rtx"],
            MustExclude = ["gtx"],
        };
        var service = new WillhabenService(_logger, config, mockHttpClient);

        // Act
        var result = await service.SearchAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result, listing =>
        {
            var fullText = ((listing.Heading ?? "") + " " + (listing.Description ?? "")).ToLower();
            Assert.True(fullText.Contains("rtx"), $"Expected 'rtx' in '{fullText}'");
            Assert.False(fullText.Contains("gtx"), $"Did not expect 'gtx' in '{fullText}'");
        });
    }

    [Fact]
    public async Task SearchAsync_StateFilter_RemovesNonMatchingStates()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient();
        var config = new WillhabenAppSettings
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            AllowedStates = ["Wien"],
        };
        var service = new WillhabenService(_logger, config, mockHttpClient);

        // Act
        var result = await service.SearchAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result, listing =>
        {
            Assert.Equal("Wien", listing.State);
        });
    }

    [Fact]
    public async Task SearchAsync_DistanceCalculation_SortsByDistance()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient();
        var config = new WillhabenAppSettings
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            SortByDistance = true,
            ReferenceLat = 48.2082,
            ReferenceLon = 16.3738,
        };
        var service = new WillhabenService(_logger, config, mockHttpClient);

        // Act
        var result = await service.SearchAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        for (var i = 1; i < result.Count; i++)
        {
            var prev = result[i - 1].DistanceKm ?? double.MaxValue;
            var curr = result[i].DistanceKm ?? double.MaxValue;
            Assert.True(prev <= curr, $"Results not sorted by distance at index {i}: {prev} > {curr}");
        }
    }

    [Fact]
    public async Task SearchAsync_MaxDistanceFilter_RemovesFarListings()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient();
        var config = new WillhabenAppSettings
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            SortByDistance = true,
            ReferenceLat = 48.2082,
            ReferenceLon = 16.3738,
            MaxDistanceKm = 10,
        };
        var service = new WillhabenService(_logger, config, mockHttpClient);

        // Act
        var result = await service.SearchAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.All(result, listing =>
        {
            Assert.NotNull(listing.DistanceKm);
            Assert.True(listing.DistanceKm <= 10, $"Distance {listing.DistanceKm} exceeds max 10 km");
        });
    }

    [Fact]
    public async Task SearchAsync_AttributeParsing_ParsesCorrectly()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient(withAttributes: true);
        var config = new WillhabenAppSettings { Category = 5882, Rows = 100 };
        var service = new WillhabenService(_logger, config, mockHttpClient);

        // Act
        var result = await service.SearchAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEmpty(result);
        var listing = result.First();
        Assert.NotNull(listing.Zustand);
        Assert.NotNull(listing.Color);
    }

    private IWillhabenHttpClient CreateMockHttpClient(bool withAttributes = false)
    {
        var listings = new List<WillhabenListingDto>
        {
            new()
            {
                AdId = "1",
                Heading = "RTX 4090 Graphics Card",
                Description = "Brand new RTX 4090, never used",
                Price = "450 EUR",
                Location = "Vienna",
                Postcode = "1010",
                State = "Wien",
                Coordinates = "48.2082,16.3738",
                CategoryTreeAttributeIds = withAttributes ? "21;22,3199;3201" : null,
                AdvertStatus = null,
                SeoUrl = "kaufen-und-verkaufen/d/rtx-4090-1/",
            },
            new()
            {
                AdId = "2",
                Heading = "RTX 4080 Super",
                Description = "Like new RTX 4080 Super, 500 km used",
                Price = "350 EUR",
                Location = "Vienna",
                Postcode = "1020",
                State = "Wien",
                Coordinates = "48.2100,16.3800",
                CategoryTreeAttributeIds = null,
                AdvertStatus = null,
                SeoUrl = "kaufen-und-verkaufen/d/rtx-4080-2/",
            },
            new()
            {
                AdId = "3",
                Heading = "RTX 4070 Ti",
                Description = "RTX 4070 Ti for sale, great condition",
                Price = "250 EUR",
                Location = "Vienna",
                Postcode = "1030",
                State = "Wien",
                Coordinates = "48.2150,16.3900",
                CategoryTreeAttributeIds = null,
                AdvertStatus = null,
                SeoUrl = "kaufen-und-verkaufen/d/rtx-4070-ti-3/",
            },
            // Reserved item
            new()
            {
                AdId = "4",
                Heading = "RTX 4060 Reserved",
                Description = "This item is reserved",
                Price = "200 EUR",
                Location = "Vienna",
                Postcode = "1040",
                State = "Wien",
                Coordinates = "48.2200,16.4000",
                CategoryTreeAttributeIds = null,
                AdvertStatus = new() { Description = "Reserviert" },
                SeoUrl = "kaufen-und-verkaufen/d/rtx-4060-4/",
            },
            // Wrong state
            new()
            {
                AdId = "5",
                Heading = "RTX 4050",
                Description = "RTX 4050 graphics card",
                Price = "150 EUR",
                Location = "Graz",
                Postcode = "8010",
                State = "Steiermark",
                Coordinates = "47.0707,15.4394",
                CategoryTreeAttributeIds = null,
                AdvertStatus = null,
                SeoUrl = "kaufen-und-verkaufen/d/rtx-4050-5/",
            },
            // Must not include (has GTX)
            new()
            {
                AdId = "6",
                Heading = "GTX 1660 Super",
                Description = "GTX 1660 Super for sale",
                Price = "100 EUR",
                Location = "Vienna",
                Postcode = "1050",
                State = "Wien",
                Coordinates = "48.2300,16.4100",
                CategoryTreeAttributeIds = null,
                AdvertStatus = null,
                SeoUrl = "kaufen-und-verkaufen/d/gtx-1660-6/",
            },
            // Price too high
            new()
            {
                AdId = "7",
                Heading = "RTX 4090 Ti",
                Description = "RTX 4090 Ti, flagship GPU",
                Price = "2000 EUR",
                Location = "Vienna",
                Postcode = "1060",
                State = "Wien",
                Coordinates = "48.2400,16.4200",
                CategoryTreeAttributeIds = null,
                AdvertStatus = null,
                SeoUrl = "kaufen-und-verkaufen/d/rtx-4090-ti-7/",
            },
        };

        var json = System.Text.Json.JsonSerializer.Serialize(listings);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content };

        var mockClient = Substitute.For<IWillhabenHttpClient>();
        mockClient.BuildSearchUrl().Returns("https://www.willhaben.at/test");
        mockClient.GetListingsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(listings));

        return mockClient;
    }
}
