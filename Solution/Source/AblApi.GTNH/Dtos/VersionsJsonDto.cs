using System.Text.Json.Serialization;

using AblApi.Common.Converters;

namespace AblApi.GTNH.Dtos;

/// <summary>
/// Represents the JSON structure returned by https://www.gtnewhorizons.com/versions.json
/// The root object is a dictionary keyed by version string (e.g. "2.8.0-rc-2").
/// </summary>
public class VersionsJsonDto : Dictionary<string, VersionsJsonEntryDto>
{
}

public class VersionsJsonEntryDto
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("releaseDate")]
    [JsonDateTimeFormat("yyyy/MM/dd")]
    public required DateTime ReleaseDate { get; set; }

    [JsonPropertyName("maxJavaVersion")]
    public required int MaxJavaVersion { get; set; }

    [JsonPropertyName("mmc")]
    public required VersionsJsonMmcDto Mmc { get; set; }

    [JsonPropertyName("server")]
    public required VersionsJsonServerDto Server { get; set; }

    [JsonPropertyName("client")]
    public required VersionsJsonClientDto Client { get; set; }

    public StableVersionDto ToDto(string version)
    {
        return new StableVersionDto
        {
            Version = version,
            CreatedAt = this.ReleaseDate,
            DownloadUrls = new DownloadUrlsDto
            {
                Client = this.Mmc.Java17_2XUrl,
                ClientJava8 = this.Mmc.Java8Url,
                Server = this.Server.Java17_2XUrl,
                ServerJava8 = this.Server.Java8Url
            }
        };
    }
}

public class VersionsJsonMmcDto
{
    [JsonPropertyName("java8Url")]
    public required string Java8Url { get; set; }

    [JsonPropertyName("java17_2XUrl")]
    public required string Java17_2XUrl { get; set; }
}

public class VersionsJsonServerDto
{
    [JsonPropertyName("java8Url")]
    public required string Java8Url { get; set; }

    [JsonPropertyName("java17_2XUrl")]
    public required string Java17_2XUrl { get; set; }
}

public class VersionsJsonClientDto
{
    [JsonPropertyName("java8Url")]
    public required string Java8Url { get; set; }
}
