using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AblApi.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IHostEnvironment environment, ILogger<GlobalExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception,
            "Unhandled exception processing {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (_environment.IsDevelopment())
        {
            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = exception.Message,
                stackTrace = exception.StackTrace
            }, cancellationToken);
        }
        else
        {
            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = "An unexpected error occurred."
            }, cancellationToken);
        }

        return true;
    }
}
