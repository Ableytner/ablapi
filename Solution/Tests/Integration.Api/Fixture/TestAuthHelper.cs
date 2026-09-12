using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Integration.Api.Fixture;

// Authentication handler which bypasses the real authentication for testing purposes
public class TestAuthHandler(
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	public const string SchemeName = "Test";

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		Claim[] claims =
		[
			new Claim("name", "Integration Test Admin User"),
			new Claim("role", "Admin")
		];

		var identity = new ClaimsIdentity(
			claims,
			SchemeName,
			nameType: "name",
			roleType: "role");
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, SchemeName);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}
