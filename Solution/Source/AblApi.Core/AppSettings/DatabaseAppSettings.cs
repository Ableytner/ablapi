using AblApi.Common.Enums;

namespace AblApi.Core.AppSettings;

public class DatabaseAppSettings
{
    public const string SectionName = "Database";

    public DatabaseType Type { get; set; }

    private string _connection;

    public string Connection
    {
        get
        {
            if (Type == DatabaseType.Sqlite && !string.IsNullOrEmpty(_connection))
            {
                return _connection.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            }

            return _connection;
        }
        set => _connection = value;
    }
}
