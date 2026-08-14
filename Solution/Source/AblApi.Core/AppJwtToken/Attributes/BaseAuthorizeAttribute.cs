using AblApi.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AblApi.Core.AppJwtToken.Attributes;

public abstract class BaseAuthorizeAttribute : AuthorizeAttribute
{
    protected BaseAuthorizeAttribute(ApiAccessRole primaryRole)
    {
        if (primaryRole == ApiAccessRole.Admin)
        {
            Roles = ApiAccessRole.Admin.ToString();
            return;
        }

        Roles = $"{primaryRole},{ApiAccessRole.Admin}";
    }

    protected BaseAuthorizeAttribute(params IEnumerable<ApiAccessRole> roles)
    {
        var allRoles = new HashSet<ApiAccessRole>(roles) { ApiAccessRole.Admin };
        Roles = string.Join(",", allRoles);
    }
}
