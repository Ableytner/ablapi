using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using AblApi.GTNH;
using AblApi.GTNH.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace AblApi.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class GTNHController(IGTNHService gtnhManager, IMemoryCache cache, GTNHAppSettings gtnhAppSettings) : ControllerBase
{
    private readonly IGTNHService _gtnhManager = gtnhManager;
    private readonly IMemoryCache _cache = cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(gtnhAppSettings.CacheExpirationMinutes);
    private readonly bool _isCachingEnabled = gtnhAppSettings.CacheExpirationMinutes > 0;

    [HttpGet("daily/latest")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [EndpointSummary("Get the latest daily GTNH version")]
    [ProducesResponseType(typeof(DailyVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyVersionDto>> GetLatestDailyVersion([FromQuery] bool? success = null)
    {
        var cacheKey = $"gtnh:daily:latest:{success}";

        if (_isCachingEnabled && _cache.TryGetValue(cacheKey, out DailyVersionDto? cachedDto))
        {
            return Ok(cachedDto);
        }

        var dailyVersion = await _gtnhManager.GetLatestDailyVersionAsync(success);

        if (dailyVersion == null)
        {
            return NotFound();
        }

        if (_isCachingEnabled)
        {
            _cache.Set(cacheKey, dailyVersion, new MemoryCacheEntryOptions { SlidingExpiration = _cacheExpiration });
        }
        return Ok(dailyVersion);
    }

    [HttpGet("daily/{runNumber:int}")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [EndpointSummary("Get a specific daily GTNH version")]
    [ProducesResponseType(typeof(DailyVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyVersionDto>> GetSpecificDailyVersion([FromRoute] int runNumber)
    {
        var dailyVersion = await _gtnhManager.GetSpecificDailyVersionAsync(runNumber);

        if (dailyVersion == null)
        {
            return NotFound();
        }
        return Ok(dailyVersion);
    }

    [HttpGet("stable/latest")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [EndpointSummary("Get the latest stable GTNH version")]
    [ProducesResponseType(typeof(StableVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StableVersionDto>> GetLatestStableVersion()
    {
        var cacheKey = "gtnh:stable:latest";

        if (_isCachingEnabled && _cache.TryGetValue(cacheKey, out StableVersionDto? cachedDto))
        {
            return Ok(cachedDto);
        }

        var stableVersion = await _gtnhManager.GetLatestStableVersionAsync();

        if (stableVersion == null)
        {
            return NotFound();
        }

        if (_isCachingEnabled)
        {
            _cache.Set(cacheKey, stableVersion, new MemoryCacheEntryOptions { SlidingExpiration = _cacheExpiration });
        }
        return Ok(stableVersion);
    }

    [HttpGet("stable/{stableVersion}")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [EndpointSummary("Get a specific stable GTNH version")]
    [ProducesResponseType(typeof(StableVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StableVersionDto>> GetSpecificStableVersion([FromRoute] string stableVersion)
    {
        var stableVersionDto = await _gtnhManager.GetSpecificStableVersionAsync(stableVersion);

        if (stableVersionDto == null)
        {
            return NotFound();
        }
        return Ok(stableVersionDto);
    }
}
