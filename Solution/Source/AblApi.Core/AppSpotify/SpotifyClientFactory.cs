using AblApi.Common.Utilities;
using SpotifyAPI.Web;

namespace AblApi.Core.AppSpotify;

public class SpotifyClientFactory(SpotifyAppSettings appSettings)
{
    private readonly SpotifyAppSettings _appSettings = appSettings;

    private SpotifyClient? _instance = null;
    private DateTime? _requestCreatedAt = null;

    public SpotifyClient? Instance { get => _instance; }

    public async Task<string> GetAuthUrl()
    {
        var loginRequest = new LoginRequest(
            GetCallbackUri(),
            _appSettings.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = [
                Scopes.PlaylistReadPrivate,
                Scopes.PlaylistReadCollaborative,
                Scopes.UserLibraryRead,
                Scopes.PlaylistModifyPublic,
                Scopes.PlaylistModifyPrivate
            ]
        };

        _requestCreatedAt = DateTime.Now;

        return loginRequest.ToUri().AbsoluteUri;
    }

    public async Task<bool> Build(string code)
    {
        if (_requestCreatedAt is null)
        {
            return false;
        }
        if ((DateTime.Now - _requestCreatedAt.Value).TotalMinutes > 15)
        {
            _requestCreatedAt = null;
            return false;
        }

        var response = await new OAuthClient().RequestToken(
            new AuthorizationCodeTokenRequest(_appSettings.ClientId, _appSettings.ClientSecret, code, GetCallbackUri())
        );

        var config = SpotifyClientConfig
                    .CreateDefault()
                    .WithAuthenticator(new AuthorizationCodeAuthenticator(_appSettings.ClientId, _appSettings.ClientSecret, response));
        _instance = new SpotifyClient(config);

        _requestCreatedAt = null;
        return true;
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
