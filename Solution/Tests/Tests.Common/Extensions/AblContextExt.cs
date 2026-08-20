using AblApi.DataAccess.Context;

namespace Tests.Common.Extensions;

public static class AblContextExt
{
	public static async Task SeedInMemory(this AblContext dbContext)
	{
		// TODO: create any objects that are needed for all tests

		await dbContext.SaveChangesAsync().ConfigureAwait(false);
	}
}
