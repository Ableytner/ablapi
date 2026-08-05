using System.Text.Json.Serialization;

namespace AblApi.Core.AppGithubApi.Dtos;

public class WorkflowArtifactsDto
{
    [JsonPropertyName("total_count")]
    public required int TotalCount { get; set; }

    [JsonPropertyName("artifacts")]
    public required WorkflowArtifactDto[] Artifacts { get; set; }
}

public class WorkflowArtifactDto
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("size_in_bytes")]
    public required long SizeInBytes { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("archive_download_url")]
    public required string ArchiveDownloadUrl { get; set; }

    [JsonPropertyName("expired")]
    public required bool Expired { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required DateTime UpdatedAt { get; set; }

    [JsonPropertyName("expires_at")]
    public required DateTime ExpiresAt { get; set; }
}
