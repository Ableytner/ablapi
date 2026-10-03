using AblApi.Api.Logging;
using AblApi.Core.AppJwtToken.Attributes;
using AblApi.Core.AppLogging.Dtos;
using AblApi.Core.AppSettings;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;

namespace AblApi.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;
    private readonly Serilog.ILogger _forwardLogger;

    public LogController(ILogger<LogController> logger, IServiceProvider services, ElkAppSettings elkConfig)
    {
        _logger = logger;

//        _forwardLogger = new LoggerConfiguration()
//                             .WriteTo.Elasticsearch(elkConfig)
//                             .WriteTo.NikoBot(services)
//                             .CreateLogger();
        _forwardLogger = new LoggerConfiguration()
                             .WriteTo.NikoBot(services)
                             .CreateLogger();
    }

    [HttpPost]
    [AuthorizeLog]
    [EndpointSummary("Send a log message to be processed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Log([FromBody] LogMessageDto message)
    {
        _logger.LogDebug("Received request to log {Severity} from {Sender}: {Content}", message.LogLevel, message.Sender, message.Message);

        using (LogContext.PushProperty("Sender", message.Sender))
        {
            _forwardLogger.Write(
                LogUtil.GetLogLevel(message.LogLevel),
                message.Message
            );
        }

        return Ok();
    }
}
