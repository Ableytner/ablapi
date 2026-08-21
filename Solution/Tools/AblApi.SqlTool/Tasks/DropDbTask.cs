using AblApi.Common.Enums;
using Microsoft.Data.Sqlite;

namespace AblApi.SqlTool.Tasks;

public class DropDbTask(AppConfig config) : BaseTask(config)
{
    public override string Name => "DropDb";

    public override string Description => "Deletes the local database.";

    public override string Command => "dropdb";

    public override void Run()
    {
        switch (Config.Database.Type)
        {
            case DatabaseType.Postgres:
                Console.WriteLine("Error: TRIED TO DROP POSTGRES DATABASE.");
                break;
            case DatabaseType.Sqlite:
                DropSqliteDb();
                break;
            default:
                throw new InvalidOperationException("Unknown database type");
        }
    }

    private void DropSqliteDb()
    {
        var builder = new SqliteConnectionStringBuilder(Config.Database.Connection);
        var dbPath = builder.DataSource;

        if (string.IsNullOrEmpty(dbPath) || !File.Exists(dbPath))
        {
            Console.WriteLine("Database file does not exist, nothing to do.");
            return;
        }

        Console.Write($"Do you really want to delete the database at \"{dbPath}\"? (y/n): ");
        var confirmation = Console.ReadLine();
        if (!string.Equals(confirmation?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            SqliteConnection.ClearAllPools();
            File.Delete(dbPath);
            Console.WriteLine($"Deleted database at \"{dbPath}\"");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Deleting the database failed: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}
