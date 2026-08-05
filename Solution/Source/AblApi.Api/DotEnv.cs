namespace AblApi.Api;

// code from: https://dusted.codes/dotenv-in-dotnet
public static class DotEnv
{
    public static void LoadEnvVariables()
    {
        var root = Directory.GetCurrentDirectory();
        // Used in prod
        DotEnv.Load(Path.GetFullPath(Path.Combine(root, ".env")));
        // Used in dev
        DotEnv.Load(Path.GetFullPath(Path.Combine(root, "..", "..", "..", ".env")));
        // Used in tests
        DotEnv.Load(Path.GetFullPath(Path.Combine(root, "..", "..", "..", "..", "..", "..", ".env")));
    }

    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}
