using AblApi.Common.Enums;

namespace AblApi.DataAccess.Models;

public partial class ApiAccessRoleGrant
{
    // PK
    public long Id { get; set; }

    public required Guid UserId { get; set; }

    public required ApiAccessRole Role { get; set; }
    
    public required DateTime GrantedAt { get; set; }

    public virtual ApiUser User { get; set; } = null!;
}
