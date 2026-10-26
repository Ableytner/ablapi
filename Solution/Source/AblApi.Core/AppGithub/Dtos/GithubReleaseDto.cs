using System.Text.Json.Serialization;

namespace AblApi.Core.AppGithub.Dtos;

public class GithubReleaseDto
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("prerelease")]
    public required bool Prerelease { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("published_at")]
    public required DateTimeOffset PublishedAt { get; set; }

    [JsonPropertyName("assets")]
    public required GithubReleaseAssetDto[] Assets { get; set; }

    [JsonPropertyName("tarball_url")]
    public required string TarballUrl { get; set; }

    [JsonPropertyName("zipball_url")]
    public required string ZipballUrl { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }
}

public class GithubReleaseAssetDto
{
    [JsonPropertyName("id")]
    public required long Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("content_type")]
    public required string ContentType { get; set; }

    [JsonPropertyName("size")]
    public required long Size { get; set; }

    [JsonPropertyName("download_count")]
    public required int DownloadCount { get; set; }

    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public required DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("browser_download_url")]
    public required string BrowserDownloadUrl { get; set; }
}
