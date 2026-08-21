using AblApi.Api.Startup;
using AblApi.Common;
using AblApi.Core.AppSettings;
using AblApi.SqlTool.Tasks;
using Elastic.CommonSchema;
using Microsoft.Extensions.Configuration;

namespace AblApi.SqlTool;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(Util.GetSeparator());
        Console.WriteLine("Sql Tool for database interfacing");
        Console.WriteLine(Util.GetSeparator());

        var config = GetAppConfig();
        var tasks = GetAllTasks(config);

        while (true)
        {
            Console.WriteLine();

            var maxCommandWidth = Math.Max("Command".Length, tasks.Max(task => task.Command.Length));
            var maxNameWidth = Math.Max("Task name".Length, tasks.Max(task => task.Name.Length));
            var maxDescriptionWidth = Math.Max("Description".Length, tasks.Max(task => task.Description.Length));

            var header = "Command".PadRight(maxCommandWidth) + " | " + "Task name".PadRight(maxNameWidth) + " | " + "Description".PadRight(maxDescriptionWidth) + " |";
            Console.WriteLine(new string('-', header.Length));
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
            foreach (var task in tasks)
            {
                Console.WriteLine(task.Command.PadRight(maxCommandWidth) + " | " + task.Name.PadRight(maxNameWidth) + " | " + task.Description.PadRight(maxDescriptionWidth) + " |");
            }
            Console.WriteLine(new string('-', header.Length));
            Console.WriteLine();

            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
                continue;

            BaseTask? targetTask = null;
            foreach (var task in tasks)
            {
                if (input == task.Command)
                {
                    targetTask = task;
                }
            }
            if (targetTask is null)
            {
                Console.WriteLine("Unknown command!");
                Console.WriteLine();
                continue;
            }

            targetTask.Run();
        }
    }

    private static AppConfig GetAppConfig()
    {
        var configuration = new ConfigurationBuilder().AddConfigProviders().Build();

        var dbConfig = new DatabaseAppSettings();
        configuration.GetSection(DatabaseAppSettings.SectionName).Bind(dbConfig);
        if (string.IsNullOrEmpty(dbConfig.Connection) || dbConfig.Connection == "DBCONNECTION")
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        return new AppConfig
        {
            Database = dbConfig
        };
    }

    private static List<BaseTask> GetAllTasks(AppConfig config)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(BaseTask)))
            .Select(type => Activator.CreateInstance(type, config) as BaseTask)
            .OfType<BaseTask>()
            .OrderBy(task => task.Name)
            .ToList();
    }
}
