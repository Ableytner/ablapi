namespace AblApi.DataAccess.Models;

public class ApiUser
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required ICollection<ApiAccessRoleGrant> Roles { get; set; } = new List<ApiAccessRoleGrant>();
}
