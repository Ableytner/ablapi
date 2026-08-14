using AblApi.Common.Enums;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeDiscordSendToOwnerAttribute : BaseAuthorizeAttribute
{
    public AuthorizeDiscordSendToOwnerAttribute() : base(ApiAccessRole.DiscordSendToOwner)
    {
    }
}
