using AblApi.Core;
using AblApi.Core.AppSettings;
using AblApi.DataAccess;
using AblApi.DataAccess.Context;
using AblApi.DataAccess.Extensions;
using AblApi.Repositories;
using AblApi.Repositories.Interfaces;

namespace AblApi.Api.Startup;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration config)
    {
        var dbConfig = new DatabaseAppSettings();
        config.GetSection(DatabaseAppSettings.SectionName).Bind(dbConfig);
        if (string.IsNullOrEmpty(dbConfig.Connection) || dbConfig.Connection == "DBCONNECTION")
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }
        services.AddSingleton(dbConfig);

        services.AddScoped<IAblRepository, AblRepository>();
        services.AddDbContext<AblContext>(options =>
        {
            options.ConfigureDatabase(dbConfig.Type, dbConfig.Connection);
        });

        if (EnvironmentHelper.IsProduction())
        {
            var migrationHandler = new DbMigrationHandler(dbConfig.Type, dbConfig.Connection);

            if (migrationHandler.NeedsMigration())
            {
                migrationHandler.Migrate();
            }
        }

        return services;
    }
}
