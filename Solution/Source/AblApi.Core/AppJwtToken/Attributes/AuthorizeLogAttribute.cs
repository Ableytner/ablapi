using AblApi.Common.Enums;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeLogAttribute : BaseAuthorizeAttribute
{
    public AuthorizeLogAttribute() : base(ApiAccessRole.Log)
    {
    }
}
