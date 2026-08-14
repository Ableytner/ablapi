using Tests.Common.Mocks;

namespace Integration.Api.Fixture;

public abstract class TestBase : IAsyncLifetime
{
    private readonly Lock _hostLock = new();
    private readonly TestApiFactory _sharedFactory;
    private readonly HttpClient _sharedClient;

    protected CancellationToken CancellationToken { get; private init; } = TestContext.Current.CancellationToken;

    protected TestHelpers TestHelpers { get; init; }

    protected TestBase()
    {
        // in-memory Sqlite database shared between test API and test code
        var testDbContext = DbContextMocker.GetSqliteContextInMemory("TestBaseDb");

        lock (_hostLock)
        {
            _sharedFactory = new(testDbContext);
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
