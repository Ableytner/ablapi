using AblApi.Common.Enums;
using AblApi.Core.AppSettings;
using AblApi.DataAccess.Context;
using AblApi.Repositories;
using AblApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            _ = dbConfig.Type switch
            {
                DatabaseType.Postgres => options.UseNpgsql(
                    dbConfig.Connection,
                    npgsql => npgsql.CommandTimeout(120)
                ),
                DatabaseType.Sqlite => options.UseSqlite(
                    dbConfig.Connection,
                    sqlite => sqlite.CommandTimeout(120)
                ),
                _ => throw new InvalidOperationException("Unknown database type")
            };
        });

        return services;
    }
}
