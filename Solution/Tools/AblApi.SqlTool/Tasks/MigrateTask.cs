using AblApi.DataAccess;

namespace AblApi.SqlTool.Tasks;

public class MigrateTask(AppConfig config) : BaseTask(config)
{
    public override string Name => "Migrate";

    public override string Description => "Migrates the database to the latest version, creates the database if it doesn't exist.";

    public override string Command => "migrate";

    private readonly DbMigrationHandler _migrationHandler = new(config.Database.Type, config.Database.Connection);

    public override void RunInteractive()
    {
        RunCi([]);
    }

    public override void RunCi(string[] _)
    {
        try
        {
            if (!_migrationHandler.NeedsMigration())
            {
                Console.WriteLine("No migration needed");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Status check failed, something is probably configured incorrectly: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            return;
        }

        try
        {
            _migrationHandler.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Migration failed: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
            return;
        }
    }
}
