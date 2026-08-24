using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("api/[controller]/")]
[ApiController]
public class StatusController : ControllerBase
{
    [HttpGet]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    public async Task<ActionResult<string>> Index()
    {
        return Ok("OK");
    }
}
