using AblApi.Common.Enums;
using AblApi.Core.AppSettings;

namespace Tests.Unit.Api;

public class DatabaseAppSettingsTests
{
    [Fact]
    public void Postgres_ConnectionString_ConvertsToKeyValue()
    {
        // Arrange
        var settings = new DatabaseAppSettings
        {
            Type = DatabaseType.Postgres,
            Connection = "postgresql://app:passwordhere@cluster-rw.namespace:5432/app"
        };

        // Act
        var result = settings.Connection;

        // Assert
        Assert.Equal("Host=cluster-rw.namespace;Port=5432;Database=app;Username=app;Password=passwordhere", result);
    }
}
