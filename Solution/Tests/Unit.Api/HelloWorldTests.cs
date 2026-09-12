using AblApi.Api.Controllers;
using Tests.Unit.Api.Mocks;

namespace Tests.Unit.Api;

public class HelloWorldTests(InMemorySqliteDbFixture fixture) : IClassFixture<InMemorySqliteDbFixture>
{
    private readonly InMemorySqliteDbFixture _fixture = fixture;

    [Fact]
    public async Task HelloWorld_ReturnsHelloWorld()
    {
        // Arrange
        HelloWorldController controller = new HelloWorldController();

        // Act
        var result = await controller.HelloWorld();

        // Assert
        Assert.Equal("Hello, World!", result);
    }
}
