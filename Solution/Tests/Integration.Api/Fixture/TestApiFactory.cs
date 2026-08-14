using AblApi.Api;
using AblApi.DataAccess.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Integration.Api.Fixture;

/// <summary>
/// A factory which creates the test API for integration tests.
/// The given ablContext is used as the database
/// </summary>
public class TestApiFactory(AblContext ablContext) : WebApplicationFactory<Program>
{
	private const string HostUrl = "https://localhost:5813";
    private readonly AblContext _ablContext = ablContext;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices((context, services) =>
		{
			SetupAuth(services);
            SetupDb(services);
		});

        builder.UseUrls(HostUrl);
	}

    private static void SetupAuth(IServiceCollection services)
    {
        services.Configure<AuthenticationOptions>(o =>
        {
            o.DefaultScheme = TestAuthHandler.SchemeName;
            o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
            o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            o.DefaultForbidScheme = TestAuthHandler.SchemeName;
            o.DefaultSignInScheme = TestAuthHandler.SchemeName;
            o.DefaultSignOutScheme = TestAuthHandler.SchemeName;
        });

        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, _ => { });
    }

    private void SetupDb(IServiceCollection services)
	{
		services.RemoveAll<DbContextOptions<AblContext>>();
		services.AddDbContext<AblContext>(_ => GetDbContext());
	}

    private AblContext GetDbContext()
    {
        return _ablContext;
    }
}
