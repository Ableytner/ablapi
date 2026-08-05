using AblApi.GTNH;
using AblApi.GTNH.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GTNHController(ILogger<GTNHController> logger, IGTNHManager gtnhManager) : ControllerBase
{
    private readonly ILogger<GTNHController> _logger = logger;
    private readonly IGTNHManager _gtnhManager = gtnhManager;

    [HttpGet("daily/latest")]
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
