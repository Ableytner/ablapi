namespace AblApi.Core.AppSettings;

public class EnvironmentIdAppSettings
{
	public const string SectionName = "EnvironmentIdentifier";

	public required string Id { get; set; }
}
