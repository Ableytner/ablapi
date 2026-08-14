using Microsoft.Extensions.Configuration;

namespace Tests.Common;

internal static class TestConfigHelper
{
	public static IConfigurationRoot GetIConfigurationRoot()
	{
		string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"; 

		return new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 
			.Build();
	}

	public static string GetConnectionString()
	{
		string connectionString = GetIConfigurationRoot().GetConnectionString("IntegrationTestDb") ??
			throw new Exception("No connection string.");
		return connectionString;
	}
}
