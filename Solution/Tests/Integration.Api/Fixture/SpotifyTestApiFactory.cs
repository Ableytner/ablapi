using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.Spotify;
using Microsoft.EntityFrameworkCore;
using Tests.Common.Mocks;

namespace Integration.Api.Fixture;

public class SpotifyTestApiFactory : TestApiFactory
{
    // The linked account whose credentials are copied into the in-memory test DB
    public const string TestUser = "650587171375284226";

    private const string InMemoryDbName = "spotify_live";

    public AblContext SeedContext { get; }

    public SpotifyTestApiFactory() : base(InMemoryDbName)
    {
        SeedContext = DbContextMocker.GetSqliteContextInMemory(InMemoryDbName);

        var linkedUser = ReadLinkedUserFromDevDb();
        SeedContext.SpotifyUsers.Add(linkedUser);
        SeedContext.SaveChanges();
    }

    private static SpotifyUser ReadLinkedUserFromDevDb()
    {
        var devConnection = "Data Source=~/Database.db;".Replace(
            "~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

        var options = new DbContextOptionsBuilder<AblContext>()
            .UseSqlite(devConnection)
            .Options;

        using var devContext = new AblContext(options);
        var user = devContext.SpotifyUsers
            .FirstOrDefault(u => u.UserIdentifier == TestUser)
            ?? throw new InvalidOperationException(
                $"No linked Spotify account for user '{TestUser}' in the dev database. " +
                "Run the OAuth flow once against the dev environment to link it.");

        // Detach so the entity can be inserted into the in-memory test context
        devContext.Entry(user).State = EntityState.Detached;

        return user;
    }
}
