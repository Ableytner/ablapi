using AblApi.Common.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace AblApi.DataAccess.Extensions;

public static class DbContextOptionsBuilderExt
{
    public static void ConfigureDatabase(this DbContextOptionsBuilder optionsBuilder, DatabaseType type, string connection)
    {
        switch (type)
        {
            case DatabaseType.Postgres:
                optionsBuilder.UseNpgsql(
                    connection,
                    npgsql => npgsql.CommandTimeout(120)
                );
                break;
            case DatabaseType.Sqlite:
                optionsBuilder.UseSqlite(
                    connection,
                    sqlite => sqlite.CommandTimeout(120)
                );
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown database type");
        }
    }
}
