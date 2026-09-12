using AblApi.DataAccess.Context;

namespace Integration.Api.Fixture;

public class TestHelpers
{
	public required TestApiFactory ApiFactory { get; init; }

	public required HttpClient Client { get; init; }

	public required AblContext AblContext { get; init; }
}
