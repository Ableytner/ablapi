using AblApi.Core.AppWillhaben.Dtos;
using AblApi.Core.AppWillhaben.Extensions;

namespace AblApi.Core.AppWillhaben.Domain;

public class WillhabenListing
{
    public required string Id { get; set; }

    public required string Heading { get; set; }

    public required string Description { get; set; }

    public required double? Price { get; set; }

    public required bool IsReserved { get; set; }

    public required string Url { get; set; }

    public string? Location { get; set; }

    public string? Postcode { get; set; }

    public string? State { get; set; }

    public string? Coordinates { get; set; }

    public string? Color { get; set; }

    public string? Zustand { get; set; }

    public string? Übergabe { get; set; }

    public int? Km { get; set; }

    public double? DistanceKm { get; set; }

    public static WillhabenListing FromDto(WillhabenListingDto dto, WillhabenAppSettings settings)
    {
        var listing = new WillhabenListing
        {
            Id = dto.Id ?? dto.AdId ?? "",
            Heading = dto.GetAttr("HEADING") ?? "",
            Description = dto.GetAttr("DESCRIPTION") ?? "",
            Price = dto.ExtractPrice(),
            IsReserved = dto.ExtractIsReserved(),
            Url = $"https://www.willhaben.at/iad/{dto.GetAttr("SEO_URL")}",
            Location = dto.GetAttr("LOCATION"),
            Postcode = dto.GetAttr("POSTCODE"),
            State = dto.GetAttr("STATE"),
            Coordinates = dto.GetAttr("COORDINATES"),
            Color = TreeAttributes.ParseValue(dto.GetAttr("CATEGORYTREEATTRIBUTEIDS"), "Farbe"),
            Zustand = TreeAttributes.ParseValue(dto.GetAttr("CATEGORYTREEATTRIBUTEIDS"), "Zustand"),
            Übergabe = TreeAttributes.ParseValue(dto.GetAttr("CATEGORYTREEATTRIBUTEIDS"), "Übergabe"),
            Km = dto.ExtractKm(),
            DistanceKm = dto.ExtractDistance(settings),
        };

        return listing;
    }
}
