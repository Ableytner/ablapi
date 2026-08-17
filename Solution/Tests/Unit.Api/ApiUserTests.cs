using AblApi.Common.Enums;
using AblApi.Common.Extensions;
using AblApi.DataAccess.Models;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Tests.Unit.Api.Mocks;

namespace Tests.Unit.Api;

public class ApiUserTests(InMemorySqliteDbFixture fixture) : IClassFixture<InMemorySqliteDbFixture>
{
    private readonly InMemorySqliteDbFixture _fixture = fixture;
    private readonly IAblRepository _ablRepository = fixture.ServiceProvider.GetService<IAblRepository>();

    [Fact]
    public async Task ApiUserRepository_Upsert_PersistsChanges()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _fixture.AblContext.Add(
            new ApiUser { Id = guid, Name = "Test User", Roles = [
                new ApiAccessRoleGrant { Role = ApiAccessRole.Admin, GrantedAt = DateTime.UtcNow },
                new ApiAccessRoleGrant { Role = ApiAccessRole.Log, GrantedAt = DateTime.UtcNow }
                ] }
            );
        var updatedUser = new ApiUser {
            Id = guid,
            Name = "Updated User",
            Roles = [
                new ApiAccessRoleGrant { Role = ApiAccessRole.Log, GrantedAt = DateTime.UtcNow }
            ]
        };
        _fixture.AblContext.SaveChanges();

        // Act
        await _ablRepository.ApiUserRepository.UpsertAsync(updatedUser);

        // Assert
        var result = await _ablRepository.ApiUserRepository.GetByIdAsync(guid);
        Assert.NotNull(result);
        Assert.Equal("Updated User", result.Name);
        var grant = Assert.Single(result.Roles);
        Assert.Equal(ApiAccessRole.Log, grant.Role);
        Assert.True(grant.GrantedAt.IsWithinSecondsOf(DateTime.UtcNow, 5));

        Assert.Equal(1, _fixture.AblContext.ApiAccessRoleGrants.Count());
    }
}
