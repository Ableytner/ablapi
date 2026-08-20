using AblApi.Common.Enums;
using DbUp;
using DbUp.Engine;
using Microsoft.Data.Sqlite;

namespace AblApi.DataAccess;

public class DbMigrationHandler
{
    private readonly DatabaseType _type;
    private readonly string _connectionString;
    private readonly UpgradeEngine _upgradeEngine;

    public DbMigrationHandler(DatabaseType type, string connectionString)
    {
        _type = type;
        _connectionString = connectionString;

        _upgradeEngine = type switch
        {
            DatabaseType.Postgres => DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(typeof(DbMigrationHandler).Assembly)
                .LogToConsole()
                .WithTransactionPerScript()
                .Build(),
            DatabaseType.Sqlite => DeployChanges.To
                .SqliteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(typeof(DbMigrationHandler).Assembly)
                .LogToConsole()
                .WithTransactionPerScript()
                .Build(),
            _ => throw new InvalidOperationException("Unknown database type")
        };
    }

    public void Migrate()
    {
        // create database if it does not exist
        switch (_type)
        {
            case DatabaseType.Postgres:
                EnsureDatabase.For.PostgresqlDatabase(_connectionString);
                break;
            case DatabaseType.Sqlite:
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                }
                break;
            default:
                throw new InvalidOperationException("Unknown database type");
        }

        int retries = 3;
        while (retries > 0)
        {
            try
            {
                if (_upgradeEngine.IsUpgradeRequired())
                {
                    DatabaseUpgradeResult result = _upgradeEngine.PerformUpgrade();
                }
                return;
            }
            catch (Exception)
            {
                Thread.Sleep(1000);
                retries--;

                if (retries == 0)
                {
                    throw;
                }
            }
        }
    }

    public bool NeedsMigration()
    {
        return _upgradeEngine.IsUpgradeRequired();
    }
}
