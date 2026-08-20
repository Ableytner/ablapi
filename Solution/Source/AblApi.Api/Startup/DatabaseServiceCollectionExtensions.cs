using AblApi.DataAccess.Context;
using AblApi.Repositories;
using AblApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AblApi.Api.Startup;

public static class DatabaseServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IAblRepository, AblRepository>();
        services.AddDbContext<AblContext>(options =>
            options.UseSqlite(
                config.GetConnectionString("DbConnection"),
                sqlite => sqlite.CommandTimeout(120))
        );

        return services;
    }
}
