using AblApi.Common;
using AblApi.Common.Utilities;
using AblApi.Core.AppWillhaben.Dtos;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AblApi.Core.AppWillhaben;

public class WillhabenHttpClient : CachedHttpClient, IWillhabenHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public WillhabenHttpClient()
    {
        BaseAddress = new Uri("https://www.willhaben.at");
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue(Encoding.UTF8.WebName));
        DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AblApi", "1.0"));
    }

    public virtual string BuildSearchUrl(WillhabenConfigDto config)
    {
        var baseUrl = "https://www.willhaben.at/iad/kaufen-und-verkaufen/marktplatz/-" + config.Category;

        var builder = new UrlBuilder()
            .WithUrl(baseUrl)
            .WithParam("rows", config.Rows.ToString())
            .WithOptionalParam(!string.IsNullOrEmpty(config.Keyword), "keyword", config.Keyword)
            .WithOptionalParam(config.PriceMin > 0, "PRICE_FROM", config.PriceMin.ToString())
            .WithOptionalParam(config.PriceMax > 0 && config.PriceMax < 1_000_000_000, "PRICE_TO", config.PriceMax.ToString())
            .WithOptionalParam(config.FilterPaylivery, "paylivery", "true");

        if (config.HandoverTypes.Count > 0)
        {
            builder.WithParam("treeAttributes", config.HandoverTypes.Select(TreeAttributes.ParseNames));
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
