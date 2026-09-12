namespace AblApi.DataAccess.Models;

public partial class SchemaVersion
{
    public required int Id { get; set; }

    public required string ScriptName { get; set; }

    public required DateTime AppliedAt { get; set; }
}
