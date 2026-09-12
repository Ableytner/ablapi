using Microsoft.Extensions.DependencyInjection;

namespace Integration.Api.Fixture;

public class RealAuthTestApiFactory(string databaseName) : TestApiFactory(databaseName)
{
    protected override void SetupAuth(IServiceCollection services)
    {
        // don't change the auth setup, use the real auth
    }
}
