using Tests.Common.Mocks;

namespace Tests.Unit.Api.Mocks;

public class InMemorySqliteDbFixture : BaseFixture
{
    public InMemorySqliteDbFixture() : base(DbContextMocker.GetSqliteContextInMemory("TestDatabase"))
    {
    }
}
