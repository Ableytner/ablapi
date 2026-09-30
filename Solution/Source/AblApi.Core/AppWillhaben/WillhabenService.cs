using AblApi.Core.AppWillhaben.Domain;
using AblApi.Core.AppWillhaben.Dtos;
using AblApi.Core.AppWillhaben.Extensions;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppWillhaben;

public class WillhabenService(ILogger<WillhabenService> logger, WillhabenAppSettings settings, IWillhabenHttpClient httpClient) : IWillhabenService
{
    private readonly ILogger<WillhabenService> _logger = logger;
    private readonly WillhabenAppSettings _settings = settings;
    private readonly IWillhabenHttpClient _httpClient = httpClient;

    public async Task<List<WillhabenListing>> SearchAsync(CancellationToken cancellationToken = default)
    {
        var url = _httpClient.BuildSearchUrl();
        _logger.LogInformation("Fetching willhaben listings from {Url}", url);

        var rawDtos = await _httpClient.GetListingsAsync(url, cancellationToken);
        _logger.LogInformation("Received {Count} raw listings from willhaben", rawDtos.Count);

        var domainListings = rawDtos.Where(FilterFunc)
                                    .Select(dto => WillhabenListing.FromDto(dto, _settings))
                                    .Where(FilterFunc)
                                    .ToList();

        // Distance filter if enabled
        if (_settings.MaxDistanceKm > 0)
        {
            domainListings = domainListings.Where(l => l.DistanceKm.HasValue && l.DistanceKm <= _settings.MaxDistanceKm)
                                           .ToList();
        }

        // Sort by distance if enabled
        if (_settings.SortByDistance)
        {
            domainListings.Sort((a, b) =>
            {
                var da = a.DistanceKm ?? double.MaxValue;
                var db = b.DistanceKm ?? double.MaxValue;
                return da.CompareTo(db);
            });
        }

        return domainListings;
    }

    private bool FilterFunc(WillhabenListingDto dto)
    {
        // Keyword filter
        var fullText = $"{dto.GetAttr("HEADING")} {dto.GetAttr("DESCRIPTION")} {dto.GetAttr("BODY_DYN")}";
        if (_settings.MustInclude.Count > 0)
        {
            if (!_settings.MustInclude.All(kw => fullText.Contains(kw, StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }
        }
        if (_settings.MustExclude.Count > 0)
        {
            if (_settings.MustExclude.Any(kw => fullText.Contains(kw, StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }
        }

        // Bundesland filter
        if (_settings.AllowedStates.Count > 0)
        {
            var state = dto.GetAttr("STATE");
            if (string.IsNullOrEmpty(state) || !_settings.AllowedStates.Contains(state))
            {
                return false;
            }
        }

        return true;
    }

    private bool FilterFunc(WillhabenListing item)
    {
        // Reserved filter
        if (item.IsReserved)
        {
            return false;
        }

        // Price filter
        if (_settings.PriceMin > 0 || _settings.PriceMax > 0)
        {
            if (item.Price.HasValue)
            {
                if ((_settings.PriceMin > 0 && item.Price < _settings.PriceMin) || (_settings.PriceMax > 0 && item.Price > _settings.PriceMax))
                {
                    return false;
                }
            }
        }

        // KM filter
        if (_settings.KmMax > 0)
        {
            if (item.Km.HasValue && item.Km > _settings.KmMax)
            {
                return false;
            }
        }

        return true;
    }
}
