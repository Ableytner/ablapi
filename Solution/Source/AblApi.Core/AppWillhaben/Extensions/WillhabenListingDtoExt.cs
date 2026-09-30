using System.Text.RegularExpressions;
using AblApi.Core.AppWillhaben.Dtos;

namespace AblApi.Core.AppWillhaben.Extensions;

public static class WillhabenListingDtoExt
{
    public static double? ExtractPrice(this WillhabenListingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Price))
        {
            return null;
        }

        var digits = dto.Price.Replace("EUR", "")
                              .Replace("€", "")
                              .Replace(",", ".")
                              .Trim()
                              .Where(c => char.IsDigit(c) || c == '.');

        return double.TryParse(string.Concat(digits), out double parsed) ? parsed : null;
    }

    public static bool ExtractIsReserved(this WillhabenListingDto dto)
    {
        if (dto.AdvertStatus != null)
        {
            var desc = (dto.AdvertStatus.Description ?? "").ToLower();
            if (desc.Contains("reserviert") || desc.Contains("reserved"))
                return true;
        }

        var fulltext = $"{dto.Heading} {dto.BodyDyn}".ToLower();
        if (Regex.IsMatch(fulltext, @"\breserviert\b"))
            return true;

        return false;
    }

    public static int? ExtractKm(this WillhabenListingDto dto)
    {
        var text = (dto.Heading ?? "") + " " + (dto.BodyDyn ?? "");

        var m1 = Regex.Match(text,
            @"(?:nur|nur\s*ca\.?|ca\.?)\s*(\d{1,5})\s*km",
            RegexOptions.IgnoreCase);
        if (m1.Success)
            return int.Parse(m1.Groups[1].Value);

        var m2 = Regex.Match(text,
            @"(?:km(?:-|–|\s*)?Stand|km gelaufen|km gefahren|km Leistung|km Laufleistung)\D{0,5}(\d{1,5})",
            RegexOptions.IgnoreCase);
        if (m2.Success)
            return int.Parse(m2.Groups[1].Value);

        var m3 = Regex.Match(text,
            @"(\d{1,5})\s*(?:km\s*(?:gelaufen|gefahren|Leistung|Laufleistung|Stand))",
            RegexOptions.IgnoreCase);
        if (m3.Success)
            return int.Parse(m3.Groups[1].Value);

        return null;
    }

    public static double? ExtractDistance(this WillhabenListingDto dto, WillhabenAppSettings settings)
    {
        if (dto.Coordinates != null)
        {
            var coords = dto.Coordinates.Split(',');
            if (coords.Length == 2 &&
                double.TryParse(coords[0], out var lat) &&
                double.TryParse(coords[1], out var lon) &&
                !double.IsNaN(lat) &&
                !double.IsNaN(lon))
            {
                return CalculateDistance(settings.ReferenceLat, settings.ReferenceLon, lat, lon);
            }
        }

        return null;
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return Math.Round(R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)) * 10) / 10;
    }
}
