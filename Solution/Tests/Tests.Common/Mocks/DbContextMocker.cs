using AblApi.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Tests.Common.Extensions;

namespace Tests.Common.Mocks;

public static class DbContextMocker
{
	private static readonly Lock _lock = new();

	public static AblContext GetSqliteContextInMemory(string dbName)
	{
		_lock.Enter();

		var options = GetSqliteOptionsInMemory(dbName);

		var dbContext = new AblContext(options);
		dbContext.Database.OpenConnection();
		dbContext.Database.EnsureCreated();
		Task.Run(async () => await dbContext.SeedInMemory()).Wait();

		_lock.Exit();

		return dbContext;
	}

	public static DbContextOptions<AblContext> GetSqliteOptionsInMemory(string dbName)
	{
		return new DbContextOptionsBuilder<AblContext>()
			.UseSqlite($"DataSource=Data/{dbName}.db;Mode=Memory;Cache=Shared")
			.ConfigureWarnings(x => {
				x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning);
			})
			.Options;
	}
}
