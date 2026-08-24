using Integration.Api.Fixture;

namespace Integration.Api.GTNH.Fixture;

public abstract class GTNHTestBase(string gtnhStableVersionApiUrl) : TestBase
{
    private readonly string _gtnhStableVersionApiUrl = gtnhStableVersionApiUrl;

    protected override TestApiFactory CreateTestApiFactory(string databaseName)
    {
        return new GTNHTestApiFactory(databaseName, _gtnhStableVersionApiUrl);
    }
}
