using AblApi.Core.AppWillhaben.Domain;
using AblApi.Core.AppWillhaben.Dtos;
using AblApi.Core.AppWillhaben.Extensions;
using AblApi.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace AblApi.Core.AppWillhaben;

public class WillhabenService(ILogger<WillhabenService> logger, IWillhabenHttpClient httpClient, IAblRepository ablRepository) : IWillhabenService
{
    private readonly ILogger<WillhabenService> _logger = logger;
    private readonly IWillhabenHttpClient _httpClient = httpClient;
    private readonly IAblRepository _ablRepository = ablRepository;

    public async Task<List<WillhabenListing>> SearchAsync(WillhabenConfigDto config, CancellationToken cancellationToken = default)
    {
        var url = _httpClient.BuildSearchUrl(config);
        _logger.LogInformation("Fetching willhaben listings from {Url}", url);

        var rawDtos = await _httpClient.GetListingsAsync(url, cancellationToken);
        _logger.LogInformation("Received {Count} raw listings from willhaben", rawDtos.Count);

        var domainListings = rawDtos.Where(dto => FilterFunc(dto, config))
                                    .Select(dto => WillhabenListing.FromDto(dto, config))
                                    .Where(item => FilterFunc(item, config))
                                    .ToList();

        // Distance filter if enabled
        if (config.MaxDistanceKm > 0)
        {
            domainListings = domainListings.Where(l => l.DistanceKm.HasValue && l.DistanceKm <= config.MaxDistanceKm)
                                           .ToList();
        }

        // Sort by distance if enabled
        if (config.SortByDistance)
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

    public async Task<WillhabenConfigDto> CreateConfigAsync(WillhabenConfigDto dto, CancellationToken cancellationToken = default)
    {
        var dbo = dto.ToDbo();
        await _ablRepository.WillhabenConfigRepository.AddAsync(dbo);
        return WillhabenConfigDto.FromDbo(dbo);
    }

    public async Task<WillhabenConfigDto> UpdateConfigAsync(WillhabenConfigDto dto, CancellationToken cancellationToken = default)
    {
        var dbo = dto.ToDbo();
        await _ablRepository.WillhabenConfigRepository.UpsertAsync(dbo);
        return WillhabenConfigDto.FromDbo(dbo);
    }

    public async Task DeleteConfigAsync(string name, CancellationToken cancellationToken = default)
    {
        await _ablRepository.WillhabenConfigRepository.RemoveByNameAsync(name);
    }

    public async Task<List<WillhabenConfigDto>> ListConfigsAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _ablRepository.WillhabenConfigRepository.GetAllAsListAsync();
        return configs.Select(WillhabenConfigDto.FromDbo).ToList();
    }

    public async Task AddSeenListingAsync(WillhabenListing listing, CancellationToken cancellationToken = default)
    {
        await _ablRepository.WillhabenSeenListingRepository.RemoveByUrlAsync(listing.Url);
        _ablRepository.WillhabenSeenListingRepository.AddListing(listing.Url, listing.Price ?? 0);
        await _ablRepository.SaveChangesAsync();
    }

    public Task<bool> HasSeenListingAsync(WillhabenListing listing, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_ablRepository.WillhabenSeenListingRepository.Exists(listing.Url, listing.Price ?? 0));
    }

    private static bool FilterFunc(WillhabenListingDto dto, WillhabenConfigDto config)
    {
        // Keyword filter
        var fullText = $"{dto.GetAttr("HEADING")} {dto.GetAttr("DESCRIPTION")} {dto.GetAttr("BODY_DYN")}";
        if (config.MustInclude.Count > 0)
        {
            if (!config.MustInclude.All(kw => fullText.Contains(kw, StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }
        }
        if (config.MustExclude.Count > 0)
        {
            if (config.MustExclude.Any(kw => fullText.Contains(kw, StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }
        }

        // Bundesland filter
        if (config.AllowedStates.Count > 0)
        {
            var state = dto.GetAttr("STATE");
            if (string.IsNullOrEmpty(state) || !config.AllowedStates.Contains(state))
            {
                return false;
            }
        }

        return true;
    }

    private static bool FilterFunc(WillhabenListing item, WillhabenConfigDto config)
    {
        // Reserved filter
        if (item.IsReserved)
        {
            return false;
        }

        // Price filter
        if (config.PriceMin > 0 || config.PriceMax > 0)
        {
            if (item.Price.HasValue)
            {
                if ((config.PriceMin > 0 && item.Price < config.PriceMin) || (config.PriceMax > 0 && item.Price > config.PriceMax))
                {
                    return false;
                }
            }
        }

        // KM filter
        if (config.KmMax > 0)
        {
            if (item.Km.HasValue && item.Km > config.KmMax)
            {
                return false;
            }
        }

        return true;
    }
}
