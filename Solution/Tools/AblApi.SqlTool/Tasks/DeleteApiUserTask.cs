using AblApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AblApi.SqlTool.Tasks;

public class DeleteUserTask(AppConfig config) : BaseTask(config)
{
    private const int IdWidth = 36;
    private const int NameWidth = 16;

    public override string Name => "DeleteUser";

    public override string Description => "Lists all ApiUsers and deletes the selected one.";

    public override string Command => "deleteuser";

    public override void RunInteractive()
    {
        using var context = CreateContext();
        using var repository = new AblRepository(context);

        var users = context.ApiUsers
            .Include(u => u.Roles)
            .OrderBy(u => u.Name)
            .ToList();

        if (!users.Any())
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

        Console.Write("Enter the name of the ApiUser to delete: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name must not be empty!");
            return;
        }

        var userToDelete = users.FirstOrDefault(u => string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));
        if (userToDelete == null)
        {
            Console.WriteLine($"No ApiUser found with name '{name}'.");
            return;
        }

        Console.Write($"Are you sure you want to delete ApiUser '{userToDelete.Name}'? (y/n): ");
        var confirmation = Console.ReadLine();
        if (!string.Equals(confirmation?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Deletion cancelled.");
            return;
        }

        repository.ApiUserRepository.Remove(userToDelete);
        repository.SaveChangesAsync().GetAwaiter().GetResult();

        Console.WriteLine($"Deleted ApiUser '{userToDelete.Name}'.");
    }

    public override void RunCi(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine($"Expected exactly one argument (the ApiUser name), got {args.Length} instead");
            return;
        }

        var name = args[0];

        using var context = CreateContext();
        using var repository = new AblRepository(context);

        var allUsers = context.ApiUsers
            .Include(u => u.Roles)
            .ToList();

        var userToDelete = allUsers.FirstOrDefault(u => string.Equals(u.Name, name, StringComparison.OrdinalIgnoreCase));

        if (userToDelete == null)
        {
            Console.WriteLine($"No ApiUser found with name '{name}'.");
            return;
        }

        repository.ApiUserRepository.Remove(userToDelete);
        repository.SaveChangesAsync().GetAwaiter().GetResult();

        Console.WriteLine($"Deleted ApiUser '{userToDelete.Name}'.");
    }
}
