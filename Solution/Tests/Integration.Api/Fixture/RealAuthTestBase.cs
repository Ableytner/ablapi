namespace Integration.Api.Fixture;

public abstract class RealAuthTestBase : TestBase
{
    protected override TestApiFactory CreateTestApiFactory(string databaseName)
    {
        return new RealAuthTestApiFactory(databaseName);
    }
}
