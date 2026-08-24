using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken;
using AblApi.DataAccess.Models;
using Integration.Api.Fixture;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;

namespace Integration.Api;

public class HelloWorldControllerTests : RealAuthTestBase
{
    private const string BaseUrl = "api/hello-world";

    private readonly IJwtTokenService _jwtTokenService;

    public HelloWorldControllerTests()
    {
        _jwtTokenService = TestHelpers.ApiFactory.Services.GetRequiredService<IJwtTokenService>();
    }

    [Fact]
    public async Task HelloWorld_PublicAccess_ReturnsSuccessWithoutAuth()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    #region Private Endpoint Tests

    [Theory]
    [InlineData(null)]
    [InlineData(ApiAccessRole.Admin)]
    public async Task HelloWorldForUsers_WithValidRole_ReturnsSuccess(ApiAccessRole? role)
    {
        // Arrange
        var user = role.HasValue 
            ? await CreateUserWithRolesAsync(new List<ApiAccessRole> { role.Value })
            : await CreateApiUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/private");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task HelloWorldForUsers_WithInvalidRole_ReturnsExpectedStatus()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/private");

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Admin Endpoint Tests

    [Fact]
    public async Task HelloWorldForAdmins_WithAdminUser_ReturnsSuccess()
    {
        // Arrange
        var user = await CreateUserWithRolesAsync(new List<ApiAccessRole> { ApiAccessRole.Admin });

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task HelloWorldForAdmins_WithMultipleRoles_ReturnsSuccess()
    {
        // Arrange
        var user = await CreateUserWithRolesAsync(new List<ApiAccessRole> { ApiAccessRole.Log, ApiAccessRole.Admin });

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    public async Task HelloWorldForAdmins_WithInvalidRole_ReturnsExpectedStatus(
        bool isRegisteredUser,
        HttpStatusCode expectedStatus)
    {
        // Arrange
        ApiUser? user = isRegisteredUser ? await CreateApiUserAsync() : null;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/admin");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    #endregion

    #region Log Endpoint Tests

    [Theory]
    [InlineData(ApiAccessRole.Log)]
    [InlineData(ApiAccessRole.Admin)]
    public async Task HelloWorldForLogClients_WithValidRole_ReturnsSuccess(ApiAccessRole role)
    {
        // Arrange
        var user = await CreateUserWithRolesAsync(new List<ApiAccessRole> { role });

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/log");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    public async Task HelloWorldForLogClients_WithInvalidRole_ReturnsExpectedStatus(
        bool isRegisteredUser,
        HttpStatusCode expectedStatus)
    {
        // Arrange
        ApiUser? user = isRegisteredUser ? await CreateApiUserAsync() : null;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/log");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    #endregion

    #region Discord Endpoint Tests

    [Theory]
    [InlineData(ApiAccessRole.DiscordSendToAll)]
    [InlineData(ApiAccessRole.Admin)]
    public async Task HelloWorldForDiscordMessageSenders_WithValidRole_ReturnsSuccess(ApiAccessRole role)
    {
        // Arrange
        var user = await CreateUserWithRolesAsync(new List<ApiAccessRole> { role });

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/discord");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData(true, HttpStatusCode.Forbidden)]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    public async Task HelloWorldForDiscordMessageSenders_WithInvalidRole_ReturnsExpectedStatus(
        bool isRegisteredUser,
        HttpStatusCode expectedStatus)
    {
        // Arrange
        ApiUser? user = isRegisteredUser ? await CreateApiUserAsync() : null;

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/discord");
        AddAuthHeader(request, user);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(expectedStatus, response.StatusCode);
    }

    #endregion

    #region Helpers

    private async Task<ApiUser> CreateApiUserAsync()
    {
        var user = new ApiUser
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Token = Guid.NewGuid().ToString()
        };

        TestHelpers.AblContext.ApiUsers.Add(user);
        await TestHelpers.AblContext.SaveChangesAsync(CancellationToken);

        return user;
    }

    private async Task<ApiUser> CreateUserWithRolesAsync(List<ApiAccessRole> roles)
    {
        var user = await CreateApiUserAsync();
        user.Name = $"Test User ({string.Join(", ", roles)})";

        foreach (var role in roles)
        {
            var roleGrant = new ApiAccessRoleGrant
            {
                UserId = user.Id,
                Role = role,
                GrantedAt = DateTime.UtcNow,
                User = user
            };

            user.Roles.Add(roleGrant);
        }

        await TestHelpers.AblContext.SaveChangesAsync(CancellationToken);

        return user;
    }

    private void AddAuthHeader(HttpRequestMessage request, ApiUser? user)
    {
        if (user == null)
        {
            return;
        }

        var roles = user.Roles.Select(r => r.Role);
        var token = _jwtTokenService.CreateToken(user.Id, roles);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
    }

    #endregion
}
