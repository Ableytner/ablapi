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
            if (Type == DatabaseType.Postgres && !string.IsNullOrEmpty(_connection))
            {
                // CNPG provides urls of the form: postgresql://app:passwordhere@cluster-rw.namespace:5432/app
                // this is converted to: Host=cluster-rw.namespace;Port=5432;Database=app;Username=app;Password=passwordhere
                var uri = new Uri(_connection);
                var builder = new Dictionary<string, string>();
                builder["Host"] = uri.Host;
                builder["Port"] = uri.Port.ToString();
                var pathParts = uri.PathAndQuery.TrimStart('/').Split('/');
                if (pathParts.Length > 0 && !string.IsNullOrEmpty(pathParts[0]))
                {
                    builder["Database"] = pathParts[0];
                }
                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var userInfoParts = uri.UserInfo.Split(':', 2);
                    builder["Username"] = userInfoParts[0];
                    if (userInfoParts.Length > 1)
                    {
                        builder["Password"] = userInfoParts[1];
                    }
                }
                return string.Join(";", builder.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }

            return _connection;
        }
        set => _connection = value;
    }
}
