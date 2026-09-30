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

        return JsonSerializer.Deserialize<List<WillhabenListingDto>>(advertSummary.GetRawText(), JsonOptions) ?? [];
    }
}
