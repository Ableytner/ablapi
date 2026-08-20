namespace AblApi.DataAccess.Models;

public class ApiUser
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<ApiAccessRoleGrant> Roles { get; set; } = new List<ApiAccessRoleGrant>();
}
