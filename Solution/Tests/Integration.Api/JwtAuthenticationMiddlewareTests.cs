using AblApi.Core.AppJwtToken;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Tests.Common.Mocks;

namespace Integration.Api;

/// <summary>
/// Integration tests that exercise the production JWT Bearer authentication middleware
/// (configured by AddAuth) directly, instead of going through the test-only
/// <see cref="TestAuthHandler"/> header-based bypass used by the other integration tests.
/// </summary>
public class JwtAuthenticationMiddlewareTests : RealAuthTestBase
{
    private const string RegisteredEndpoint = "api/hello-world/private/";
    private const string AdminEndpoint = "api/hello-world/admin/";

    private readonly JwtAppSettings _jwtSettings;
    private readonly IJwtTokenService _jwtTokenService;

    public JwtAuthenticationMiddlewareTests()
    {
        _jwtSettings = TestHelpers.ApiFactory.Services.GetRequiredService<JwtAppSettings>();
        _jwtTokenService = TestHelpers.ApiFactory.Services.GetRequiredService<IJwtTokenService>();
    }

    [Fact]
    public async Task ValidToken_AllowsAccessToRegisteredEndpoint()
    {
        // Arrange
        var token = _jwtTokenService.CreateToken(Guid.NewGuid(), []);
        var request = CreateRequest(RegisteredEndpoint, token.Token);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ValidTokenWithAdminRole_AllowsAccessToAdminEndpoint()
    {
        // Arrange
        var token = _jwtTokenService.CreateToken(Guid.NewGuid(), [AblApi.Common.Enums.ApiAccessRole.Admin]);
        var request = CreateRequest(AdminEndpoint, token.Token);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ValidTokenWithoutAdminRole_ReturnsForbidden()
    {
        // Arrange
        var token = _jwtTokenService.CreateToken(Guid.NewGuid(), []);
        var request = CreateRequest(AdminEndpoint, token.Token);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MissingAuthorizationHeader_ReturnsUnauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, RegisteredEndpoint);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MalformedToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateRequest(RegisteredEndpoint, "this-is-not-a-jwt");

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var expiredToken = JwtTokenMocker.CreateCustomJwtToken(
            key: _jwtSettings.Key,
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(-5));
        var request = CreateRequest(RegisteredEndpoint, expiredToken);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenSignedWithWrongKey_ReturnsUnauthorized()
    {
        // Arrange
        var tamperedToken = JwtTokenMocker.CreateCustomJwtToken(
            key: "this-is-a-completely-different-signing-key-value",
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(30));
        var request = CreateRequest(RegisteredEndpoint, tamperedToken);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenWithWrongIssuer_ReturnsUnauthorized()
    {
        // Arrange
        var token = JwtTokenMocker.CreateCustomJwtToken(
            key: _jwtSettings.Key,
            issuer: "https://not-the-real-issuer.example",
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(30));
        var request = CreateRequest(RegisteredEndpoint, token);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenWithWrongAudience_ReturnsUnauthorized()
    {
        // Arrange
        var token = JwtTokenMocker.CreateCustomJwtToken(
            key: _jwtSettings.Key,
            issuer: _jwtSettings.Issuer,
            audience: "not-the-real-audience",
            expires: DateTime.UtcNow.AddMinutes(30));
        var request = CreateRequest(RegisteredEndpoint, token);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #region Helpers

    private static HttpRequestMessage CreateRequest(string url, string bearerToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        return request;
    }

    #endregion
}
