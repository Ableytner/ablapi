using AblApi.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tests.Common.Extensions;

namespace Tests.Common.Mocks;

public static class DbContextMocker
{
	public static AblContext GetSqliteContextInMemory(string dbName)
	{
		ILogger<AblContext> logger = Substitute.For<ILogger<AblContext>>();

		var options = new DbContextOptionsBuilder<AblContext>()
			.UseSqlite($"DataSource=Data/{dbName}.db;Mode=Memory;Cache=Shared")
			.ConfigureWarnings(x => {
				x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning);
            })
			.Options;

		var dbContext = new AblContext(options, logger);
		dbContext.Database.OpenConnection();
		dbContext.Database.EnsureCreated();
		Task.Run(async () => await dbContext.SeedInMemory()).Wait();

		return dbContext;
	}
}
