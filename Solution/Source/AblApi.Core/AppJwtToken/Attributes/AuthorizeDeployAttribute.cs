using AblApi.Common.Enums;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeDeployAttribute : BaseAuthorizeAttribute
{
    public AuthorizeDeployAttribute() : base(ApiAccessRole.Deploy)
    {
    }
}
