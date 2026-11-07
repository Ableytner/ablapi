using AblApi.Common.Enums;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeAdminAttribute : BaseAuthorizeAttribute
{
    public AuthorizeAdminAttribute() : base(ApiAccessRole.Admin)
    {
    }
}
