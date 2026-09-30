using System.Text.Json.Serialization;

namespace AblApi.Core.AppWillhaben.Dtos;

public class WillhabenListingDto
{
    [JsonPropertyName("adId")]
    public string? AdId { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("advertStatus")]
    public AdvertStatusDto? AdvertStatus { get; set; }

    [JsonPropertyName("attributes")]
    public AttributesDto? Attributes { get; set; }
}

public class AttributesDto
{
    [JsonPropertyName("attribute")]
    public List<AttributeDto>? Attribute { get; set; }
}

public class AttributeDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("values")]
    public List<string?>? Values { get; set; }
}

public class AdvertStatusDto
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
