namespace AblApi.GTNH;

public class GTNHAppSettings
{
	public const string SectionName = "GTNHConfig";

	public int FetchCycleInSeconds { get; set; }

	public string StableVersionApiUrl { get; set; }

    public string DailyBuildsRepoOwner { get; set; }

    public string DailyBuildsRepoName { get; set; }

	public string DailyBuildsWorkflowId { get; set; }

	public int CacheExpirationMinutes { get; set; }

    public string WebMapRepoOwner { get; set; }

    public string WebMapRepoName { get; set; }
}
