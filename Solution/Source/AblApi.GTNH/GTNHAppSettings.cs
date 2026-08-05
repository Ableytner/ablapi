namespace AblApi.GTNH;

public class GTNHAppSettings
{
	public const string SectionName = "GTNHConfig";

	public int FetchCycleInSeconds { get; set; } = 60 * 60 * 1;
	public string StableVersionApiUrl { get; set; } = "";
    public string DailyBuildsRepoOwner { get; set; } = "";
    public string DailyBuildsRepoName { get; set; } = "";
	public string DailyBuildsWorkflowId { get; set; } = "";
}
