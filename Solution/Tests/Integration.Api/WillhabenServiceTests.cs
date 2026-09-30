using AblApi.Core.AppWillhaben;
using AblApi.Core.AppWillhaben.Dtos;
using AblApi.Core.AppWillhaben.Extensions;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Integration.Api;

public class WillhabenServiceTests : TestBase
{
    private readonly ILogger<WillhabenService> _logger;
    private readonly WillhabenConfigDto _config;

    public WillhabenServiceTests()
    {
        _logger = Substitute.For<ILogger<WillhabenService>>();

        _config = new WillhabenConfigDto
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
        var service = new WillhabenService(_logger, mockHttpClient);

        // Act
        var result = await service.SearchAsync(_config, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task SearchAsync_PriceFilter_RemovesOutOfRangeListings()
    {
        // Arrange
        var mockHttpClient = CreateMockHttpClient();
        var config = new WillhabenConfigDto
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            PriceMin = 200,
            PriceMax = 300,
        };
        var service = new WillhabenService(_logger, mockHttpClient);

        // Act
        var result = await service.SearchAsync(config, TestContext.Current.CancellationToken);

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
        var config = new WillhabenConfigDto
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            MustInclude = ["rtx"],
            MustExclude = ["gtx"],
        };
        var service = new WillhabenService(_logger, mockHttpClient);

        // Act
        var result = await service.SearchAsync(config, TestContext.Current.CancellationToken);

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
        var config = new WillhabenConfigDto
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            AllowedStates = ["Wien"],
        };
        var service = new WillhabenService(_logger, mockHttpClient);

        // Act
        var result = await service.SearchAsync(config, TestContext.Current.CancellationToken);

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
        var config = new WillhabenConfigDto
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            SortByDistance = true,
            ReferenceLat = 48.2082,
            ReferenceLon = 16.3738,
        };
        var service = new WillhabenService(_logger, mockHttpClient);

        // Act
        var result = await service.SearchAsync(config, TestContext.Current.CancellationToken);

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
        var config = new WillhabenConfigDto
        {
            Keyword = "test",
            Category = 5882,
            Rows = 100,
            SortByDistance = true,
            ReferenceLat = 48.2082,
            ReferenceLon = 16.3738,
            MaxDistanceKm = 10,
        };
        var service = new WillhabenService(_logger, mockHttpClient);

        // Act
        var result = await service.SearchAsync(config, TestContext.Current.CancellationToken);

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
        var config = new WillhabenConfigDto { Category = 5882, Rows = 100 };
        var service = new WillhabenService(_logger, mockHttpClient);

        // Act
        var result = await service.SearchAsync(config, TestContext.Current.CancellationToken);

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
            CreateListing("1", "RTX 4090 Graphics Card", "Brand new RTX 4090, never used", "450 EUR", "Vienna", "1010", "Wien", "48.2082,16.3738", "kaufen-und-verkaufen/d/rtx-4090-1/", withAttributes ? "21;22,3199;3201" : null, null),
            CreateListing("2", "RTX 4080 Super", "Like new RTX 4080 Super, 500 km used", "350 EUR", "Vienna", "1020", "Wien", "48.2100,16.3800", "kaufen-und-verkaufen/d/rtx-4080-2/", null, null),
            CreateListing("3", "RTX 4070 Ti", "RTX 4070 Ti for sale, great condition", "250 EUR", "Vienna", "1030", "Wien", "48.2150,16.3900", "kaufen-und-verkaufen/d/rtx-4070-ti-3/", null, null),
            // Reserved item
            CreateListing("4", "RTX 4060 Reserved", "This item is reserved", "200 EUR", "Vienna", "1040", "Wien", "48.2200,16.4000", "kaufen-und-verkaufen/d/rtx-4060-4/", null, new AdvertStatusDto { Description = "Reserviert" }),
            // Wrong state
            CreateListing("5", "RTX 4050", "RTX 4050 graphics card", "150 EUR", "Graz", "8010", "Steiermark", "47.0707,15.4394", "kaufen-und-verkaufen/d/rtx-4050-5/", null, null),
            // Must not include (has GTX)
            CreateListing("6", "GTX 1660 Super", "GTX 1660 Super for sale", "100 EUR", "Vienna", "1050", "Wien", "48.2300,16.4100", "kaufen-und-verkaufen/d/gtx-1660-6/", null, null),
            // Price too high
            CreateListing("7", "RTX 4090 Ti", "RTX 4090 Ti, flagship GPU", "2000 EUR", "Vienna", "1060", "Wien", "48.2400,16.4200", "kaufen-und-verkaufen/d/rtx-4090-ti-7/", null, null),
        };

        var json = System.Text.Json.JsonSerializer.Serialize(listings);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content };

        var mockClient = Substitute.For<IWillhabenHttpClient>();
        mockClient.BuildSearchUrl(Arg.Any<WillhabenConfigDto>()).Returns("https://www.willhaben.at/test");
        mockClient.GetListingsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(listings));

        return mockClient;
    }

    private static WillhabenListingDto CreateListing(string adId, string heading, string description, string price,
        string location, string postcode, string state, string coordinates, string seoUrl, string? categoryTreeAttributeIds,
        AdvertStatusDto? advertStatus)
    {
        var attributes = new List<AttributeDto>
        {
            new() { Name = "HEADING", Values = new List<string?> { heading } },
            new() { Name = "DESCRIPTION", Values = new List<string?> { description } },
            new() { Name = "PRICE", Values = new List<string?> { price } },
            new() { Name = "LOCATION", Values = new List<string?> { location } },
            new() { Name = "POSTCODE", Values = new List<string?> { postcode } },
            new() { Name = "STATE", Values = new List<string?> { state } },
            new() { Name = "COORDINATES", Values = new List<string?> { coordinates } },
            new() { Name = "SEO_URL", Values = new List<string?> { seoUrl } },
        };

        if (!string.IsNullOrEmpty(categoryTreeAttributeIds))
        {
            attributes.Add(new() { Name = "CATEGORYTREEATTRIBUTEIDS", Values = new List<string?> { categoryTreeAttributeIds } });
        }

        return new WillhabenListingDto
        {
            AdId = adId,
            AdvertStatus = advertStatus,
            Attributes = new AttributesDto { Attribute = attributes },
        };
    }
}
