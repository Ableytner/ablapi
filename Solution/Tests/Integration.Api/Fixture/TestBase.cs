using Tests.Common.Mocks;

namespace Integration.Api.Fixture;

public abstract class TestBase : IAsyncLifetime
{
    private readonly TestApiFactory _sharedFactory;
    private readonly HttpClient _sharedClient;

    protected CancellationToken CancellationToken { get; private init; } = TestContext.Current.CancellationToken;

    protected TestHelpers TestHelpers { get; init; }

    protected TestBase()
    {
        string databaseName = $"TestBaseDb_{Guid.NewGuid():N}";

        _sharedFactory = CreateTestApiFactory(databaseName);
        _sharedClient = _sharedFactory.CreateClient();

        TestHelpers = new()
        {
            ApiFactory = _sharedFactory,
            Client = _sharedClient,
            AblContext = DbContextMocker.GetSqliteContextInMemory(databaseName),
        };
    }

    protected virtual TestApiFactory CreateTestApiFactory(string databaseName)
    {
        return new TestApiFactory(databaseName);
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _sharedClient.Dispose();
        _sharedFactory.Dispose();
        await TestHelpers.AblContext.DisposeAsync();
    }
}
