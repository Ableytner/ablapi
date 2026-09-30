using AblApi.Common.Utilities;

namespace Tests.Unit.Api;

public class UrlBuilderTests
{
    [Fact]
    public void Build_WithoutUrl_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new UrlBuilder().Build());
    }

    [Fact]
    public void Build_WithUrlOnly_ReturnsUrl()
    {
        // Arrange & Act
        var url = new UrlBuilder().WithUrl("https://api.example.com/data").Build();

        // Assert
        Assert.Equal("https://api.example.com/data", url);
    }

    [Fact]
    public void Build_WithRequiredParam_AppendsQueryString()
    {
        // Arrange & Act
        var url = new UrlBuilder()
            .WithUrl("https://api.example.com/data")
            .WithParam("format", "json")
            .Build();

        // Assert
        Assert.Equal("https://api.example.com/data?format=json", url);
    }

    [Fact]
    public void Build_WithMultipleParams_JoinsWithAnd()
    {
        // Arrange & Act
        var url = new UrlBuilder()
            .WithUrl("https://api.example.com/data")
            .WithParam("format", "json")
            .WithParam("page", "1")
            .Build();

        // Assert
        Assert.Equal("https://api.example.com/data?format=json&page=1", url);
    }

    [Fact]
    public void Build_WithOptionalParam_True_IncludesParam()
    {
        // Arrange & Act
        var url = new UrlBuilder()
            .WithUrl("https://api.example.com/data")
            .WithOptionalParam(true, "extras", "true")
            .Build();

        // Assert
        Assert.Equal("https://api.example.com/data?extras=true", url);
    }

    [Fact]
    public void Build_WithOptionalParam_False_OmitsParam()
    {
        // Arrange & Act
        var url = new UrlBuilder()
            .WithUrl("https://api.example.com/data")
            .WithOptionalParam(false, "extras", "true")
            .Build();

        // Assert
        Assert.Equal("https://api.example.com/data", url);
    }

    [Fact]
    public void Build_WithMixedParams_CombinesCorrectly()
    {
        // Arrange & Act
        var url = new UrlBuilder()
            .WithUrl("https://api.example.com/data")
            .WithParam("format", "json")
            .WithOptionalParam(true, "extras", "true")
            .WithOptionalParam(false, "skip", "10")
            .Build();

        // Assert
        Assert.Equal("https://api.example.com/data?format=json&extras=true", url);
    }

    [Fact]
    public void Build_WithSpecialCharacters_UrlEncodesParams()
    {
        // Arrange & Act
        var url = new UrlBuilder()
            .WithUrl("https://api.example.com/data")
            .WithParam("q", "hello world")
            .Build();

        // Assert
        Assert.Equal("https://api.example.com/data?q=hello%20world", url);
    }
}
