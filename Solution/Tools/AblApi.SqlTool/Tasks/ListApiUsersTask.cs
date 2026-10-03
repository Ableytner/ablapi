using AblApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AblApi.SqlTool.Tasks;

public class ListUsersTask(AppConfig config) : BaseTask(config)
{
    private const int IdWidth = 36;
    private const int NameWidth = 16;

    public override string Name => "ListUsers";

    public override string Description => "Lists all registered ApiUsers and their roles.";

    public override string Command => "users";

    public override void RunInteractive()
    {
        ListUsers();
    }

    public override void RunCi(string[] args)
    {
        ListUsers();
    }

    private void ListUsers()
    {
        using var context = CreateContext();
        using var repository = new AblRepository(context);

        var users = context.ApiUsers
            .Include(u => u.Roles)
            .OrderBy(u => u.Name)
            .ToList();

        if (users.Count == 0)
        {
            Console.WriteLine("No ApiUsers registered.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"Id",-IdWidth}  {"Name",-NameWidth}  Roles");
        int maxRolesLen = users.Max(u => string.Join(", ", u.Roles.Select(r => r.Role.ToString())).Length);
        Console.WriteLine(new string(
            '-',
            Math.Max(
                IdWidth + 2 + NameWidth + 2 + maxRolesLen,
                $"{"Id",-IdWidth}  {"Name",-NameWidth}  Roles".Length)
            )
        );
        foreach (var user in users)
        {
            var roles = string.Join(", ", user.Roles.Select(r => r.Role.ToString()));
            Console.WriteLine($"{user.Id,IdWidth}  {user.Name,-NameWidth}  {roles}");
        }
    }
}
