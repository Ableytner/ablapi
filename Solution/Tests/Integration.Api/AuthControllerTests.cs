using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken.Dtos;
using AblApi.DataAccess.Models;
using Integration.Api.Fixture;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Integration.Api;

public class AuthControllerTests : TestBase
{
    private const string BaseUrl = "api/auth";

    [Fact]
    public async Task Authenticate_WithValidUser_ReturnsJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userToken = Guid.NewGuid().ToString();
        var testUser = new ApiUser
        {
            Id = userId,
            Name = "TestUser",
            Token = userToken,
            Roles = new List<ApiAccessRoleGrant>()
        };
        TestHelpers.AblContext.ApiUsers.Add(testUser);

        testUser.Roles.Add(new ApiAccessRoleGrant
        {
            UserId = userId,
            Role = ApiAccessRole.Admin,
            GrantedAt = DateTime.UtcNow,
            User = testUser
        });
        testUser.Roles.Add(new ApiAccessRoleGrant
        {
            UserId = userId,
            Role = ApiAccessRole.Log,
            GrantedAt = DateTime.UtcNow,
            User = testUser
        });

        await TestHelpers.AblContext.SaveChangesAsync(CancellationToken);

        var request = BuildAuthenticationRequest($"{BaseUrl}/", testUser.Id, userToken);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var jwtToken = await response.Content.ReadFromJsonAsync<JwtTokenDto>(CancellationToken);
        Assert.NotNull(jwtToken);
        Assert.NotNull(jwtToken.Token);
        Assert.NotEmpty(jwtToken.Token);
        Assert.True(jwtToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Authenticate_WithNonExistentUser_ReturnsForbid()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        var nonExistentUserToken = Guid.NewGuid().ToString();
        var request = BuildAuthenticationRequest($"{BaseUrl}/", nonExistentUserId, nonExistentUserToken);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Authenticate_WithUserWithoutRoles_ReturnsJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userToken = Guid.NewGuid().ToString();
        var testUser = new ApiUser
        {
            Id = userId,
            Name = "TestUserNoRoles",
            Token = userToken,
            Roles = new List<ApiAccessRoleGrant>()
        };
        TestHelpers.AblContext.ApiUsers.Add(testUser);
        await TestHelpers.AblContext.SaveChangesAsync(CancellationToken);

        var request = BuildAuthenticationRequest($"{BaseUrl}/", testUser.Id, userToken);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
        var jwtToken = await response.Content.ReadFromJsonAsync<JwtTokenDto>(CancellationToken);
        Assert.NotNull(jwtToken);
        Assert.NotNull(jwtToken.Token);
        Assert.NotEmpty(jwtToken.Token);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("12345")]
    [InlineData("invalid-user-id")]
    [InlineData("")]
    public async Task Authenticate_WithInvalidGuidFormat_ReturnsNotFound(string invalidUserId)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/");
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{invalidUserId}:sometoken"))
        );

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("wrong-token")]
    [InlineData("")]
    [InlineData("ab")]
    public async Task Authenticate_WithWrongToken_ReturnsForbid(string wrongToken)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userToken = Guid.NewGuid().ToString();
        var testUser = new ApiUser
        {
            Id = userId,
            Name = "TestUserWrongToken",
            Token = userToken,
            Roles = new List<ApiAccessRoleGrant>()
        };
        TestHelpers.AblContext.ApiUsers.Add(testUser);
        await TestHelpers.AblContext.SaveChangesAsync(CancellationToken);

        var request = BuildAuthenticationRequest($"{BaseUrl}/", testUser.Id, wrongToken);

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #region Helpers

    private static HttpRequestMessage BuildAuthenticationRequest(string url, Guid userId, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}:{token}"))
        );
        return request;
    }

    #endregion
}
