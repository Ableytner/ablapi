using AblApi.Common.Enums;
using AblApi.DataAccess.Context;
using AblApi.DataAccess.Extensions;
using AblApi.DataAccess.Models;
using AblApi.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;

namespace AblApi.SqlTool.Tasks;

public class CreateApiUserTask(AppConfig config) : BaseTask(config)
{
    public override string Name => "CreateApiUser";

    public override string Description => "Creates a new ApiUser with the specified grants.";

    public override string Command => "createuser";

    public override void Run()
    {
        Console.Write("Enter ApiUser name: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name must not be empty!");
            return;
        }

        List<ApiAccessRole> selectedRoles;
        while (true)
        {
            var queriedRoles = QueryRoles();
            if (queriedRoles == null)
            {
                return;
            }

            Console.Write($"Do you really want to create the user \"{name}\" with roles [{string.Join(", ", queriedRoles)}]? (y/n): ");
            var confirmation = Console.ReadLine();
            if (string.Equals(confirmation?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                selectedRoles = queriedRoles;
                break;
            }

            Console.WriteLine("Let's try again.");
        }

        var user = new ApiUser
        {
            Id = Guid.NewGuid(),
            Name = name,
            Token = GenerateToken()
        };
        var grantedAt = DateTime.UtcNow;
        foreach (var role in selectedRoles)
        {
            user.Roles.Add(new ApiAccessRoleGrant
            {
                Role = role,
                GrantedAt = grantedAt
            });
        }

        try
        {
            using var context = CreateContext();
            using var repository = new AblRepository(context);

            repository.ApiUserRepository.Add(user);
            repository.SaveChangesAsync().GetAwaiter().GetResult();

            Console.WriteLine();
            Console.WriteLine($"Created ApiUser {user.Name} with roles: {string.Join(", ", selectedRoles)}");
            Console.WriteLine($"Id: {user.Id}");
            Console.WriteLine($"Token: {user.Token}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Creating the ApiUser failed: " + ex.Message);
            Console.WriteLine(ex.StackTrace);

            var inner = ex.InnerException;
            while (inner != null)
            {
                Console.WriteLine();
                Console.WriteLine("Caused by: " + inner.Message);
                Console.WriteLine(inner.StackTrace);
                inner = inner.InnerException;
            }

            throw;
        }
    }

    private static List<ApiAccessRole>? QueryRoles()
    {
        var roles = Enum.GetValues<ApiAccessRole>();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Available roles:");
            for (var i = 0; i < roles.Length; i++)
            {
                Console.WriteLine($"{i + 1, 2}: {roles[i]}");
            }
            Console.WriteLine();

            Console.Write("Enter the numbers of the roles to grant (e.g. \"1 3 4\"): ");
            var rolesInput = Console.ReadLine();

            var selectedRoles = new List<ApiAccessRole>();
            var isValid = true;
            foreach (var part in rolesInput.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(part, out var number) || number < 1 || number > roles.Length)
                {
                    Console.WriteLine($"Invalid role number: {part}");
                    isValid = false;
                    break;
                }

                var role = roles[number - 1];
                if (!selectedRoles.Contains(role))
                {
                    selectedRoles.Add(role);
                }
            }

            if (!isValid)
            {
                continue;
            }

            return selectedRoles;
        }
    }

    private AblContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AblContext>();
        optionsBuilder.ConfigureDatabase(Config.Database.Type, Config.Database.Connection);

        return new AblContext(optionsBuilder.Options, NullLogger<AblContext>.Instance);
    }

    private static string GenerateToken()
    {
        // use the OS CSPRNG
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}
