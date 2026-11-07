using AblApi.Common.Enums;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeSpotifyAttribute : BaseAuthorizeAttribute
{
    public AuthorizeSpotifyAttribute() : base(ApiAccessRole.Spotify)
    {
    }
}
