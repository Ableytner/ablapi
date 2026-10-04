using AblApi.Core.AppWillhaben;
using AblApi.Core.AppWillhaben.Domain;
using AblApi.Core.AppWillhaben.Dtos;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using Tests.Unit.Api.Mocks;

namespace Tests.Unit.Api;

public class WillhabenServiceTests(InMemorySqliteDbFixture fixture) : IClassFixture<InMemorySqliteDbFixture>
{
    private readonly InMemorySqliteDbFixture _fixture = fixture;
    private readonly IAblRepository _ablRepository = fixture.ServiceProvider.GetRequiredService<IAblRepository>();
    private readonly ILogger<WillhabenService> _logger = Substitute.For<ILogger<WillhabenService>>();
    private readonly IWillhabenHttpClient _mockHttpClient = Substitute.For<IWillhabenHttpClient>();

    [Fact]
    public async Task CreateAsync_PersistsNewConfig()
    {
        // Arrange
        var dto = CreateTestDto("create-test");

        // Act
        var dbo = dto.ToDbo();
        await _ablRepository.WillhabenConfigRepository.AddAsync(dbo);

        // Assert
        var result = await _ablRepository.WillhabenConfigRepository.GetByNameAsync("create-test");
        Assert.NotNull(result);
        Assert.Equal("create-test", result.Name);
        Assert.Equal("bmw", result.Keyword);
        Assert.Equal(50, result.Rows);
        Assert.Equal(5000, result.PriceMin);
        Assert.Equal(20000, result.PriceMax);
        Assert.Equal(0, result.FilterPaylivery);
        AssertJsonList(result.HandoverTypes, "Abholung", "Lieferung");
        AssertJsonList(result.AllowedStates, "Wien", "Niederosterrich");
        Assert.Equal(100000, result.KmMax);
        AssertJsonList(result.MustInclude, "automatik");
        AssertJsonList(result.MustExclude, "unfall");
        Assert.True(result.SortByDistance);
        Assert.Equal(48.2082, result.ReferenceLat);
        Assert.Equal(16.3738, result.ReferenceLon);
        Assert.Equal(50, result.MaxDistanceKm);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task UpsertAsync_UpdatesExistingConfig()
    {
        // Arrange
        var original = CreateTestDto("update-test");
        var dbo = original.ToDbo();
        await _ablRepository.WillhabenConfigRepository.AddAsync(dbo);

        var updatedDto = CreateTestDto("update-test");
        updatedDto.Keyword = "audi";
        updatedDto.PriceMax = 30000;
        updatedDto.MustInclude = ["diesel", "kombi"];

        // Act
        var updatedDbo = updatedDto.ToDbo();
        await _ablRepository.WillhabenConfigRepository.UpsertAsync(updatedDbo);

        // Assert
        var result = await _ablRepository.WillhabenConfigRepository.GetByNameAsync("update-test");
        Assert.NotNull(result);
        Assert.Equal("audi", result.Keyword);
        Assert.Equal(30000, result.PriceMax);
        AssertJsonList(result.MustInclude, "diesel", "kombi");
        // Unchanged fields should remain
        Assert.Equal("update-test", result.Name);
        Assert.Equal(50, result.Rows);
    }

    [Fact]
    public async Task UpsertAsync_AddsWhenNotFound()
    {
        // Arrange
        var dto = CreateTestDto("new-via-update");

        // Act
        var dbo = dto.ToDbo();
        await _ablRepository.WillhabenConfigRepository.UpsertAsync(dbo);

        // Assert
        var result = await _ablRepository.WillhabenConfigRepository.GetByNameAsync("new-via-update");
        Assert.NotNull(result);
        Assert.Equal("new-via-update", result.Name);
        Assert.Equal("bmw", result.Keyword);
    }

    [Fact]
    public async Task DeleteAsync_RemovesConfig()
    {
        // Arrange
        var dto = CreateTestDto("delete-test");
        var dbo = dto.ToDbo();
        await _ablRepository.WillhabenConfigRepository.AddAsync(dbo);

        var existing = await _ablRepository.WillhabenConfigRepository.GetByNameAsync("delete-test");
        Assert.NotNull(existing);

        // Act
        await _ablRepository.WillhabenConfigRepository.RemoveByNameAsync("delete-test");

        // Assert
        var deleted = await _ablRepository.WillhabenConfigRepository.GetByNameAsync("delete-test");
        Assert.Null(deleted);
    }

    [Fact]
    public async Task ListAsync_ReturnsAllConfigs()
    {
        // Arrange
        await _ablRepository.WillhabenConfigRepository.AddAsync(CreateTestDto("list-test-1").ToDbo());
        await _ablRepository.WillhabenConfigRepository.AddAsync(CreateTestDto("list-test-2").ToDbo());

        // Act
        var configs = await _ablRepository.WillhabenConfigRepository.GetAllAsListAsync();

        // Assert
        var names = configs.Select(c => c.Name).OrderBy(n => n).ToList();
        Assert.Contains("list-test-1", names);
        Assert.Contains("list-test-2", names);
    }

    [Fact]
    public async Task AddSeenListingAsync_AddsNewListing()
    {
        // Arrange
        var service = CreateService();
        var listing = new WillhabenListing
        {
            Id = "test-1",
            Heading = "Test Listing",
            Description = "Test Description",
            Price = 100,
            IsReserved = false,
            Url = "https://www.willhaben.at/iad/test-url-add",
        };

        // Act
        await service.AddSeenListingAsync(listing, TestContext.Current.CancellationToken);

        // Assert
        var exists = await service.HasSeenListingAsync(listing, TestContext.Current.CancellationToken);
        Assert.True(exists);
    }

    [Fact]
    public async Task AddSeenListingAsync_ReplacesExistingListingWithSameUrl()
    {
        // Arrange
        var service = CreateService();
        var listing1 = new WillhabenListing
        {
            Id = "test-1",
            Heading = "Test Listing 1",
            Description = "Test Description",
            Price = 100,
            IsReserved = false,
            Url = "https://www.willhaben.at/iad/test-url-replace",
        };
        var listing2 = new WillhabenListing
        {
            Id = "test-2",
            Heading = "Test Listing 2",
            Description = "Test Description",
            Price = 200,
            IsReserved = false,
            Url = "https://www.willhaben.at/iad/test-url-replace",
        };

        await service.AddSeenListingAsync(listing1, TestContext.Current.CancellationToken);

        // Act
        await service.AddSeenListingAsync(listing2, TestContext.Current.CancellationToken);

        // Assert - should have the new price, not the old one
        var exists = await service.HasSeenListingAsync(listing2, TestContext.Current.CancellationToken);
        Assert.True(exists);
        var oldExists = await service.HasSeenListingAsync(listing1, TestContext.Current.CancellationToken);
        Assert.False(oldExists);
    }

    [Fact]
    public async Task HasSeenListingAsync_ReturnsTrueWhenListingExists()
    {
        // Arrange
        var service = CreateService();
        var listing = new WillhabenListing
        {
            Id = "test-1",
            Heading = "Test Listing",
            Description = "Test Description",
            Price = 100,
            IsReserved = false,
            Url = "https://www.willhaben.at/iad/test-url-exists",
        };
        await service.AddSeenListingAsync(listing, TestContext.Current.CancellationToken);

        // Act
        var result = await service.HasSeenListingAsync(listing, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasSeenListingAsync_ReturnsFalseWhenListingDoesNotExist()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.HasSeenListingAsync(new WillhabenListing
        {
            Id = "nonexistent",
            Heading = "Test",
            Description = "Test",
            Price = 100,
            IsReserved = false,
            Url = "https://www.willhaben.at/iad/nonexistent",
        }, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result);
    }

    private WillhabenService CreateService()
    {
        return new WillhabenService(_logger, _mockHttpClient, _ablRepository);
    }

    private static WillhabenConfigDto CreateTestDto(string name = "test-config") =>
        new()
        {
            Name = name,
            Keyword = "bmw",
            Category = 1,
            Rows = 50,
            PriceMin = 5000,
            PriceMax = 20000,
            FilterPaylivery = false,
            HandoverTypes = ["Abholung", "Lieferung"],
            AllowedStates = ["Wien", "Niederosterrich"],
            KmMax = 100000,
            MustInclude = ["automatik"],
            MustExclude = ["unfall"],
            SortByDistance = true,
            ReferenceLat = 48.2082,
            ReferenceLon = 16.3738,
            MaxDistanceKm = 50,
        };

    private static void AssertJsonList(string json, params string[] expected)
    {
        var actual = JsonSerializer.Deserialize<List<string>>(json) ?? [];
        Assert.Equal(expected, actual);
    }
}
