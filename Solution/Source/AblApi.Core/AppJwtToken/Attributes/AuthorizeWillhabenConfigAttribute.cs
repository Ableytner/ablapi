using AblApi.Common.Enums;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeWillhabenConfigAttribute : BaseAuthorizeAttribute
{
    public AuthorizeWillhabenConfigAttribute() : base(ApiAccessRole.WillhabenConfig)
    {
    }
}
