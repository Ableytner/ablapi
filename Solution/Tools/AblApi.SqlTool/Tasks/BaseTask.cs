using AblApi.DataAccess.Context;
using AblApi.DataAccess.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AblApi.SqlTool.Tasks;

public abstract class BaseTask(AppConfig config)
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract string Command { get; }

    protected AppConfig Config { get; } = config;

    public abstract void RunInteractive();

    public abstract void RunCi(string[] args);

    protected AblContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AblContext>();
        optionsBuilder.ConfigureDatabase(Config.Database.Type, Config.Database.Connection);

        return new AblContext(optionsBuilder.Options);
    }
}
