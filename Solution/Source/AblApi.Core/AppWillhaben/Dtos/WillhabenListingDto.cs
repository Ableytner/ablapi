using System.Text.Json.Serialization;

namespace AblApi.Core.AppWillhaben.Dtos;

public class WillhabenListingDto
{
    [JsonPropertyName("adId")]
    public string? AdId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("heading")]
    public string? Heading { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("body_dyn")]
    public string? BodyDyn { get; set; }

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("postcode")]
    public string? Postcode { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("coordinates")]
    public string? Coordinates { get; set; }

    [JsonPropertyName("categorytreeattributeids")]
    public string? CategoryTreeAttributeIds { get; set; }

    [JsonPropertyName("advertStatus")]
    public AdvertStatusDto? AdvertStatus { get; set; }

    public string? SeoUrl { get; set; }
}

public class AdvertStatusDto
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
