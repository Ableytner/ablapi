using AblApi.DataAccess.Context;
using AblApi.DataAccess.Models.Spotify;
using Microsoft.EntityFrameworkCore;
using Tests.Common.Mocks;

namespace Integration.Api.Fixture;

public class SpotifyRegisterTestApiFactory : RealAuthTestApiFactory
{
    // The api user id the linked spotify account is re-keyed under for live tests.
    public static readonly Guid TestApiUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private const string InMemoryDbName = "spotify_register_live";

    public SpotifyRegisterTestApiFactory() : base(InMemoryDbName)
    {
        var seedContext = DbContextMocker.GetSqliteContextInMemory(InMemoryDbName);

        var linkedUser = ReadLinkedUserFromDevDb();
        // re-key the linked account under the api user id so the JWT-derived identity matches
        linkedUser.UserIdentifier = TestApiUserId.ToString();
        linkedUser.AllPlaylistId = null;

        seedContext.SpotifyUsers.Add(linkedUser);
        seedContext.SaveChanges();
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
            .FirstOrDefault(u => u.UserIdentifier == SpotifyTestApiFactory.TestUser)
            ?? throw new InvalidOperationException(
                $"No linked Spotify account for user '{SpotifyTestApiFactory.TestUser}' in the dev database. " +
                "Run the OAuth flow once against the dev environment to link it.");

        // Detach so the entity can be inserted into the in-memory test context
        devContext.Entry(user).State = EntityState.Detached;

        return user;
    }
}
