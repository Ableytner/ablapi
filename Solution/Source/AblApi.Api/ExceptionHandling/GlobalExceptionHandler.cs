using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using AblApi.Core.AppSpotify.Exceptions;

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
        if (exception is UserNotLinkedException)
        {
            await ReplyAsync(httpContext, StatusCodes.Status404NotFound, "No linked Spotify account for this user.", cancellationToken);
            return true;
        }

        _logger.LogError(exception,
            "Unhandled exception processing {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        if (_environment.IsDevelopment())
        {
            await ReplyAsync(httpContext, StatusCodes.Status500InternalServerError, null, cancellationToken, exception);
        }
        else
        {
            await ReplyAsync(httpContext, StatusCodes.Status500InternalServerError, "An unexpected error occurred.", cancellationToken);
        }

        return true;
    }

    private static async Task ReplyAsync(
        HttpContext httpContext,
        int statusCode,
        string? message,
        CancellationToken cancellationToken,
        Exception? exception = null)
    {
        httpContext.Response.StatusCode = statusCode;

        if (exception is not null)
        {
            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = exception.Message,
                stackTrace = exception.StackTrace
            }, cancellationToken);
            return;
        }

        await httpContext.Response.WriteAsJsonAsync(new
        {
            message = message
        }, cancellationToken);
    }
}
