using Tests.Common.Mocks;

namespace Integration.Api.Fixture;

public abstract class TestBase : IAsyncLifetime
{
    private readonly Lock _hostLock = new();
    private readonly TestApiFactory _sharedFactory;
    private readonly HttpClient _sharedClient;

    protected CancellationToken CancellationToken { get; private init; } = TestContext.Current.CancellationToken;

    protected TestHelpers TestHelpers { get; init; }

    protected TestBase(bool useRealAuth = false)
    {
        // in-memory Sqlite database shared between test API and test code
        // uses a unique database name per test instance to ensure isolation
        var uniqueDbName = $"TestBaseDb_{Guid.NewGuid():N}";
        var testDbContext = DbContextMocker.GetSqliteContextInMemory(uniqueDbName);

        lock (_hostLock)
        {
            _sharedFactory = new(uniqueDbName, useRealAuth);
            _sharedClient = _sharedFactory.CreateClient();
        }

        TestHelpers = new()
        {
            ApiFactory = _sharedFactory,
            Client = _sharedClient,
            AblContext = testDbContext,
        };
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
