using AblApi.Common.Enums;

namespace AblApi.DataAccess.Models;

public partial class ApiAccessRoleGrant
{
    // PK
    public long Id { get; set; }

    public Guid UserId { get; set; }

    public ApiAccessRole Role { get; set; }
    
    public DateTime GrantedAt { get; set; }

    public virtual ApiUser User { get; set; } = null!;
}
