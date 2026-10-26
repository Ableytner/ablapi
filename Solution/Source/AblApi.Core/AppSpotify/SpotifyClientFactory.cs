using AblApi.Common.Utilities;
using AblApi.Core.AppSpotify.Exceptions;
using AblApi.DataAccess.Models.Spotify;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;
using System.Collections.Concurrent;

namespace AblApi.Core.AppSpotify;

public class SpotifyClientFactory(SpotifyAppSettings appSettings, IServiceScopeFactory scopeFactory)
{
    private const int AccessTokenLifetimeSeconds = 3600;

    private readonly SpotifyAppSettings _appSettings = appSettings;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    private readonly ConcurrentDictionary<string, SpotifyClient> _clients = new();
    private readonly ConcurrentDictionary<string, DateTime> _requestCreatedAt = new();

    public bool HasClient(string user)
    {
        return _clients.ContainsKey(user);
    }

    public SpotifyClient GetClient(string user)
    {
        if (!_clients.TryGetValue(user, out var client))
        {
            throw new UserNotLinkedException($"No linked Spotify account for user '{user}'.");
        }

        return client;
    }

    public async Task<string> GetAuthUrl(string user)
    {
        var loginRequest = new LoginRequest(
            GetCallbackUri(),
            _appSettings.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            State = user,
            Scope = [
                Scopes.PlaylistReadPrivate,
                Scopes.PlaylistReadCollaborative,
                Scopes.UserLibraryRead,
                Scopes.PlaylistModifyPublic,
                Scopes.PlaylistModifyPrivate
            ]
        };

        _requestCreatedAt[user] = DateTime.UtcNow;

        return loginRequest.ToUri().AbsoluteUri;
    }

    public async Task<bool> Build(string code, string user)
    {
        if (!_requestCreatedAt.TryGetValue(user, out var requestCreatedAt))
        {
            return false;
        }
        if ((DateTime.UtcNow - requestCreatedAt).TotalMinutes > 15)
        {
            _requestCreatedAt.TryRemove(user, out _);
            return false;
        }

        var response = await new OAuthClient().RequestToken(
            new AuthorizationCodeTokenRequest(_appSettings.ClientId, _appSettings.ClientSecret, code, GetCallbackUri())
        );

        var client = CreateClient(response);
        _clients[user] = client;
        _requestCreatedAt.TryRemove(user, out _);

        var spotifyUserDto = await client.UserProfile.Current();
        var spotifyUser = new SpotifyUser
        {
            UserIdentifier = user,
            SpotifyId = spotifyUserDto.Id,
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken ?? string.Empty,
            LinkedAt = DateTime.UtcNow,
        };

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAblRepository>();
        await repository.SpotifyUserRepository.UpsertAsync(spotifyUser);

        return true;
    }

    // Rebuilds all clients from the stored links, so they survive an app restart.
    public async Task RestoreAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAblRepository>();
        var users = await repository.SpotifyUserRepository.GetAllAsListAsync();

        foreach (var linkedUser in users)
        {
            var token = new AuthorizationCodeTokenResponse
            {
                AccessToken = linkedUser.AccessToken,
                TokenType = "Bearer",
                ExpiresIn = AccessTokenLifetimeSeconds,
                RefreshToken = linkedUser.RefreshToken,
                CreatedAt = linkedUser.LinkedAt,
            };

            _clients[linkedUser.UserIdentifier] = CreateClient(token);
        }
    }

    private SpotifyClient CreateClient(AuthorizationCodeTokenResponse token)
    {
        var config = SpotifyClientConfig
            .CreateDefault()
            .WithAuthenticator(new AuthorizationCodeAuthenticator(_appSettings.ClientId, _appSettings.ClientSecret, token));

        return new SpotifyClient(config);
    }

    private static Uri GetCallbackUri()
    {
        if (EnvironmentHelper.IsDevelopment())
        {
            return new Uri("http://127.0.0.1:44331/spotify/oauth/complete");
        }

        return new Uri("https://api.ableytner.at/spotify/oauth/complete");
    }
}
