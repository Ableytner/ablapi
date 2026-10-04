using AblApi.DataAccess.Models.Willhaben;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AblApi.Core.AppWillhaben.Dtos;

public class WillhabenConfigDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public int Category { get; set; }

    [JsonPropertyName("rows")]
    public int Rows { get; set; } = 100;

    [JsonPropertyName("price_min")]
    public int PriceMin { get; set; }

    [JsonPropertyName("price_max")]
    public int PriceMax { get; set; }

    [JsonPropertyName("filter_paylivery")]
    public bool FilterPaylivery { get; set; }

    [JsonPropertyName("handover_types")]
    public List<string> HandoverTypes { get; set; } = [];

    [JsonPropertyName("allowed_states")]
    public List<string> AllowedStates { get; set; } = [];

    [JsonPropertyName("km_max")]
    public int KmMax { get; set; }

    [JsonPropertyName("must_include")]
    public List<string> MustInclude { get; set; } = [];

    [JsonPropertyName("must_exclude")]
    public List<string> MustExclude { get; set; } = [];

    [JsonPropertyName("sort_by_distance")]
    public bool SortByDistance { get; set; }

    [JsonPropertyName("reference_lat")]
    public double ReferenceLat { get; set; }

    [JsonPropertyName("reference_lon")]
    public double ReferenceLon { get; set; }

    [JsonPropertyName("max_distance_km")]
    public double MaxDistanceKm { get; set; }

    public WillhabenConfig ToDbo()
    {
        return new WillhabenConfig
        {
            Id = this.Id,
            Name = this.Name,
            Keyword = this.Keyword,
            Category = this.Category,
            Rows = this.Rows,
            PriceMin = this.PriceMin,
            PriceMax = this.PriceMax,
            FilterPaylivery = this.FilterPaylivery ? 1 : 0,
            HandoverTypes = SerializeList(this.HandoverTypes),
            AllowedStates = SerializeList(this.AllowedStates),
            KmMax = this.KmMax,
            MustInclude = SerializeList(this.MustInclude),
            MustExclude = SerializeList(this.MustExclude),
            SortByDistance = this.SortByDistance,
            ReferenceLat = this.ReferenceLat,
            ReferenceLon = this.ReferenceLon,
            MaxDistanceKm = this.MaxDistanceKm,
            IsActive = true,
        };
    }

    public static WillhabenConfigDto FromDbo(WillhabenConfig config)
    {
        return new WillhabenConfigDto
        {
            Id = config.Id,
            Name = config.Name,
            Keyword = config.Keyword,
            Category = config.Category,
            Rows = config.Rows,
            PriceMin = config.PriceMin,
            PriceMax = config.PriceMax,
            FilterPaylivery = config.FilterPaylivery == 1 ? true : false,
            HandoverTypes = DeserializeList(config.HandoverTypes),
            AllowedStates = DeserializeList(config.AllowedStates),
            KmMax = config.KmMax,
            MustInclude = DeserializeList(config.MustInclude),
            MustExclude = DeserializeList(config.MustExclude),
            SortByDistance = config.SortByDistance,
            ReferenceLat = config.ReferenceLat,
            ReferenceLon = config.ReferenceLon,
            MaxDistanceKm = config.MaxDistanceKm,
        };
    }

    private static string SerializeList(List<string> list) =>
        JsonSerializer.Serialize(list);

    private static List<string> DeserializeList(string json) =>
        string.IsNullOrEmpty(json) ? [] : JsonSerializer.Deserialize<List<string>>(json) ?? [];
}
