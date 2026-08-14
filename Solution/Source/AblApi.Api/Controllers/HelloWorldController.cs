using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("api/hello-world/")]
[ApiController]
public class HelloWorldController : ControllerBase
{
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
    [HttpGet]
    public async Task<string> HelloWorld()
    {
        return "Hello, World!";
    }

    [HttpGet]
    [Route("private/")]
    [AuthorizeRegistered]
    public async Task<string> HelloWorldForUsers()
    {
        return $"Hello, registered user!";
    }

    [HttpGet]
    [Route("private/")]
    [AuthorizeAdmin]
    public async Task<string> HelloWorldForAdmins()
    {
        return $"Hello, Admin!";
    }
}
