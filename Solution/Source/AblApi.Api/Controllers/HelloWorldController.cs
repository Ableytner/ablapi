using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("api/hello-world")]
[ApiController]
public class HelloWorldController : ControllerBase
{
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [HttpGet]
    public async Task<string> HelloWorld()
    {
        return "Hello, World!";
    }
}
