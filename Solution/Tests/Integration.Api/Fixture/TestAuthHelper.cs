using Integration.Api.Mock;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Integration.Api.Fixture;

/// <summary>
/// Bypasses real Azure AD B2C authentication in integration tests.
/// Returns a pre-configured ClaimsPrincipal that satisfies all authorization
/// policies registered by AddAuth() — including ScopeRequirementHandler.
/// </summary>
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string ProfileHeaderName = MockAuthProfiles.ConfiguratorProfile;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
		var profile = Request.Headers.TryGetValue(ProfileHeaderName, out var profileHeader)
			? profileHeader.ToString()
			: MockAuthProfiles.ConfiguratorProfile;

		Claim[] claims = profile switch
        {
            MockAuthProfiles.NoClaimProfile => MockAuthProfiles.NoClaim().ToArray(),
			MockAuthProfiles.ConfiguratorProfile => MockAuthProfiles.Configurator().ToArray(),
            _ => throw new NotImplementedException($"Unknown auth profile: {profile}")
		};

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
