using AblApi.Common.Enums;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeDiscordSendToAllAttribute : BaseAuthorizeAttribute
{
    public AuthorizeDiscordSendToAllAttribute() : base(ApiAccessRole.DiscordSendToAll)
    {
    }
}
