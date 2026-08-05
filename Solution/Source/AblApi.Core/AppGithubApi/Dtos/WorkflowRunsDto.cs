using System.Text.Json.Serialization;

namespace AblApi.Core.AppGithubApi.Dtos;

public class WorkflowRunsDto
{
    [JsonPropertyName("total_count")]
    public required int TotalCount { get; set; }

    [JsonPropertyName("workflow_runs")]
    public required WorkflowRunDto[] WorkflowRuns { get; set; }
}

public class WorkflowRunDto
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("run_number")]
    public required int RunNumber { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("conclusion")]
    public required string? Conclusion { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("html_url")]
    public required string HtmlUrl { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required DateTime UpdatedAt { get; set; }
}
