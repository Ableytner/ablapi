using AblApi.Common;
using AblApi.Common.Utilities;
using AblApi.Core.AppWillhaben.Dtos;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AblApi.Core.AppWillhaben;

public class WillhabenHttpClient : CachedHttpClient, IWillhabenHttpClient
{
    private readonly WillhabenAppSettings _settings;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WillhabenHttpClient(WillhabenAppSettings settings)
    {
        _settings = settings;

        BaseAddress = new Uri("https://www.willhaben.at");
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue(Encoding.UTF8.WebName));
        DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AblApi", "1.0"));
    }

    public virtual string BuildSearchUrl()
    {
        var baseUrl = "https://www.willhaben.at/iad/kaufen-und-verkaufen/marktplatz/-" + _settings.Category;

        var builder = new UrlBuilder()
            .WithUrl(baseUrl)
            .WithParam("rows", _settings.Rows.ToString())
            .WithOptionalParam(!string.IsNullOrEmpty(_settings.Keyword), "keyword", _settings.Keyword)
            .WithOptionalParam(_settings.PriceMin > 0, "PRICE_FROM", _settings.PriceMin.ToString())
            .WithOptionalParam(_settings.PriceMax > 0 && _settings.PriceMax < 1_000_000_000, "PRICE_TO", _settings.PriceMax.ToString())
            .WithOptionalParam(_settings.FilterPaylivery, "paylivery", "true");

        if (_settings.HandoverTypes.Count > 0)
        {
            builder.WithParam("treeAttributes", _settings.HandoverTypes.Select(TreeAttributes.ParseNames));
        }

        return builder.Build();
    }

    public virtual async Task<List<WillhabenListingDto>> GetListingsAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        // Extract the embedded JSON from the __NEXT_DATA__ script tag
        var scriptTag = "<script id=\"__NEXT_DATA__\" type=\"application/json\">";
        var startIndex = html.IndexOf(scriptTag, StringComparison.Ordinal);
        if (startIndex < 0)
        {
            return [];
        }

        startIndex += scriptTag.Length;
        var endIndex = html.IndexOf("</script>", startIndex, StringComparison.Ordinal);
        if (endIndex < 0)
        {
            return [];
        }

        var jsonContent = html[startIndex..endIndex];

        using var doc = JsonDocument.Parse(jsonContent);
        var root = doc.RootElement;

        // Navigate to the listings array
        var advertSummary = root
            .GetProperty("props")
            .GetProperty("pageProps")
            .GetProperty("searchResult")
            .GetProperty("advertSummaryList")
            .GetProperty("advertSummary");

        string? GetAttribute(string key, Dictionary<string, string> attributes) =>
            attributes.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) ? value : null;

        var listings = new List<WillhabenListingDto>();

        foreach (var item in advertSummary.EnumerateArray())
        {
            // Flatten the attributes.attribute array into a dictionary
            var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (item.TryGetProperty("attributes", out var attributesElement) &&
                attributesElement.TryGetProperty("attribute", out var attributeArray))
            {
                foreach (var attr in attributeArray.EnumerateArray())
                {
                    var name = attr.GetProperty("name").GetString();
                    var values = attr.GetProperty("values");
                    if (!string.IsNullOrEmpty(name) && values.GetArrayLength() > 0)
                    {
                        attributes[name] = values[0].GetString() ?? string.Empty;
                    }
                }
            }

            // Extract top-level advertStatus before flattening overwrites it
            var topLevelAdvertStatus = item.TryGetProperty("advertStatus", out var advertStatusElement)
                ? advertStatusElement.GetRawText()
                : null;

            var dto = new WillhabenListingDto
            {
                AdId = item.TryGetProperty("adId", out var adIdElement) ? adIdElement.GetString() : null,
                Id = item.TryGetProperty("id", out var idElement) ? idElement.GetString() : null,
                Heading = GetAttribute("HEADING", attributes),
                Description = GetAttribute("DESCRIPTION", attributes),
                BodyDyn = GetAttribute("BODY_DYN", attributes),
                Price = GetAttribute("PRICE", attributes),
                Location = GetAttribute("LOCATION", attributes),
                Postcode = GetAttribute("POSTCODE", attributes),
                State = GetAttribute("STATE", attributes),
                Coordinates = GetAttribute("COORDINATES", attributes),
                CategoryTreeAttributeIds = GetAttribute("CATEGORYTREEATTRIBUTEIDS", attributes),
                AdvertStatus = ParseAdvertStatus(topLevelAdvertStatus),
                SeoUrl = GetAttribute("SEO_URL", attributes),
            };

            listings.Add(dto);
        }

        return listings;
    }

    private static AdvertStatusDto? ParseAdvertStatus(string? rawAdvertStatus)
    {
        if (string.IsNullOrEmpty(rawAdvertStatus))
            return null;

        // If it's a JSON object, deserialize it
        if (rawAdvertStatus.StartsWith("{"))
        {
            return JsonSerializer.Deserialize<AdvertStatusDto>(rawAdvertStatus, JsonOptions);
        }

        // Otherwise treat it as a plain description string
        return new AdvertStatusDto { Description = rawAdvertStatus };
    }
}
