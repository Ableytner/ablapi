using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using AblApi.GTNH;
using AblApi.GTNH.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GTNHController(ILogger<GTNHController> logger, IGTNHService gtnhManager) : ControllerBase
{
    private readonly ILogger<GTNHController> _logger = logger;
    private readonly IGTNHService _gtnhManager = gtnhManager;

    [HttpGet("daily/latest")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    public async Task<ActionResult<DailyVersionDto>> GetLatestDailyVersion([FromQuery] bool? success = null)
    {
        var dailyVersion = await _gtnhManager.GetLatestDailyVersionAsync(success);

        if (dailyVersion == null)
        {
            return NotFound();
        }
        return Ok(dailyVersion);
    }

    [HttpGet("daily/{dailyVersionId}")]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    public async Task<ActionResult<DailyVersionDto>> GetSpecificDailyVersion([FromRoute] int dailyVersionId)
    {
        var dailyVersion = await _gtnhManager.GetSpecificDailyVersionAsync(dailyVersionId);

        if (dailyVersion == null)
        {
            return NotFound();
        }
        return Ok(dailyVersion);
    }
}
