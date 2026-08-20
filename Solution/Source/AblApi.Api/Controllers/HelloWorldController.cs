using AblApi.Common.Attributes;
using AblApi.Common.Enums;
using AblApi.Core.AppJwtToken.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace AblApi.Api.Controllers;

[Route("api/hello-world/")]
[ApiController]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    [AccessLevel(AccessLevelType.PublicInternetAccess)]
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
    [Route("admin/")]
    [AuthorizeAdmin]
    public async Task<string> HelloWorldForAdmins()
    {
        return $"Hello, Admin!";
    }

    [HttpGet]
    [Route("log/")]
    [AuthorizeLog]
    public async Task<string> HelloWorldForLogClients()
    {
        return $"Hello, Log client!";
    }

    [HttpGet]
    [Route("discord/")]
    [AuthorizeDiscordSendToAll]
    public async Task<string> HelloWorldForDiscordMessageSenders()
    {
        return $"Hello, Discord message sender!";
    }
}
