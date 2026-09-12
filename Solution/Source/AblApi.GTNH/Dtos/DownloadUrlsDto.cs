using System.Text.Json.Serialization;

namespace AblApi.GTNH.Dtos;

public class DownloadUrlsDto
{
    [JsonPropertyName("client")]
    public required string Client { get; set; }

    [JsonPropertyName("client_java8")]
    public required string ClientJava8 { get; set; }

    [JsonPropertyName("server")]
    public required string Server { get; set; }

    [JsonPropertyName("server_java8")]
    public required string ServerJava8 { get; set; }
}
