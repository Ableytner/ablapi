using System.Security.Claims;

namespace Integration.Api.Mock;

public static class MockAuthProfiles
{
	public const string ConfiguratorProfile = "Configurator";
	public const string NoClaimProfile = "NoClaim";

	public static IEnumerable<Claim> Configurator() =>
	[
		new Claim("name", "Integration Test Configurator User"),
		new Claim("scp", "Backend.Application.ManageConfigurations Backend.Application.ManageSystemAccess")
	];

	public static IEnumerable<Claim> NoClaim() =>
	[
		new Claim("name", "Integration Test User Without Permissions")
	];
}
