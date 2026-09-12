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
