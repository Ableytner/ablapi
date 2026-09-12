using AblApi.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AblApi.Core.AppJwtToken.Attributes;

public class AuthorizeAdminAttribute : BaseAuthorizeAttribute
{
    public AuthorizeAdminAttribute() : base(ApiAccessRole.Admin)
    {
    }
}
