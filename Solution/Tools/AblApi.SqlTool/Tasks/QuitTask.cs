namespace AblApi.SqlTool.Tasks;

public class QuitTask(AppConfig config) : BaseTask(config)
{
    public override string Name => "Quit";

    public override string Description => "Quits the application.";

    public override string Command => "q";

    public override void Run(string[] args)
    {
        Environment.Exit(0);
    }
}
