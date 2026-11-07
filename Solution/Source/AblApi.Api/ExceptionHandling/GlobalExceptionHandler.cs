using AblApi.Core.AppSpotify.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace AblApi.Api.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is UserNotLinkedException)
        {
            await ReplyAsync(httpContext, StatusCodes.Status404NotFound, "No linked Spotify account for this user.", cancellationToken);
            return true;
        }

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
