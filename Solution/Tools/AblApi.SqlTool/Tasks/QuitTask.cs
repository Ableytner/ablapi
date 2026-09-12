namespace AblApi.SqlTool.Tasks;

public class QuitTask(AppConfig config) : BaseTask(config)
{
    public override string Name => "Quit";

    public override string Description => "Quits the application.";

    public override string Command => "q";

    public override void RunInteractive()
    {
        Environment.Exit(0);
    }

    public override void RunCi(string[] _)
    {
        Environment.Exit(0);
    }
}
