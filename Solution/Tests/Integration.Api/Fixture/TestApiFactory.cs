using AblApi.Api;
using AblApi.DataAccess.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tests.Common.Mocks;

namespace Integration.Api.Fixture;

public class TestApiFactory(string databaseName, bool useRealAuth = false) : WebApplicationFactory<Program>
{
	private const string HostUrl = "https://localhost:5813";
	private readonly string _databaseName = databaseName;
	private readonly bool _useRealAuth = useRealAuth;

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices((context, services) =>
		{
			if (!_useRealAuth)
			{
				SetupAuth(services);
			}

			SetupDb(services);
		});

		builder.UseUrls(HostUrl);
	}

	private static void SetupAuth(IServiceCollection services)
	{
		// Remove the JWT Bearer authentication that was added by AddAuth
		var jwtBearerDescriptor = services.FirstOrDefault(d => 
			d.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>));
		if (jwtBearerDescriptor != null)
		{
			services.Remove(jwtBearerDescriptor);
		}

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
		services.RemoveAll<AblContext>();

		// Register DbContext options that will create separate instances sharing the same in-memory database
		var options = DbContextMocker.GetSqliteOptionsInMemory(_databaseName);

		services.AddScoped<AblContext>(sp =>
		{
			var logger = sp.GetService<ILogger<AblContext>>() ?? Substitute.For<ILogger<AblContext>>();
			return new AblContext(options, logger);
		});
	}
}
