using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken.Dtos;
using AblApi.DataAccess.Models;
using Integration.Api.Fixture;
using System.Net;
using System.Net.Http.Json;

namespace Integration.Api;

public class AuthControllerTests : TestBase
{
    private const string BaseUrl = "api/auth";

    [Fact]
    public async Task Authenticate_WithValidUser_ReturnsJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = new ApiUser
        {
            Id = userId,
            Name = "TestUser",
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{userId}/");

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
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{nonExistentUserId}/");

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
        var testUser = new ApiUser
        {
            Id = userId,
            Name = "TestUserNoRoles",
            Roles = new List<ApiAccessRoleGrant>()
        };
        TestHelpers.AblContext.ApiUsers.Add(testUser);
        await TestHelpers.AblContext.SaveChangesAsync(CancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{userId}/");

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
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{invalidUserId}/");

        // Act
        var response = await TestHelpers.Client.SendAsync(request, CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
