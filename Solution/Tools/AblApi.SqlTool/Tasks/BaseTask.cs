namespace AblApi.SqlTool.Tasks;

public abstract class BaseTask(AppConfig config)
{
    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract string Command { get; }

    protected AppConfig Config { get; } = config;

    public abstract void Run();
}
