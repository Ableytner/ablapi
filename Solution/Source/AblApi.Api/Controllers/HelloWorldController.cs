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
    [EndpointSummary("Get a public greeting")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<string> HelloWorld()
    {
        return "Hello, World!";
    }

    [HttpGet]
    [Route("private/")]
    [AuthorizeRegistered]
    [EndpointSummary("Get a greeting for registered users")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<string> HelloWorldForUsers()
    {
        return $"Hello, registered user!";
    }

    [HttpGet]
    [Route("admin/")]
    [AuthorizeAdmin]
    [EndpointSummary("Get a greeting for admin users")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<string> HelloWorldForAdmins()
    {
        return $"Hello, Admin!";
    }

    [HttpGet]
    [Route("log/")]
    [AuthorizeLog]
    [EndpointSummary("Get a greeting for log clients")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<string> HelloWorldForLogClients()
    {
        return $"Hello, Log client!";
    }

    [HttpGet]
    [Route("discord/")]
    [AuthorizeDiscordSendToAll]
    [EndpointSummary("Get a greeting for Discord message senders")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<string> HelloWorldForDiscordMessageSenders()
    {
        return $"Hello, Discord message sender!";
    }
}
