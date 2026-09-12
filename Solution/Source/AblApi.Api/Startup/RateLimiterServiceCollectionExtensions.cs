using System.Threading.RateLimiting;

namespace AblApi.Api.Startup;

internal static class RateLimiterServiceCollectionExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsync("Too many authentication attempts. Please try again later.", cancellationToken: token);
            };

            options.AddPolicy("auth", httpContext =>
            {
                if (!httpContext.Request.Path.Equals("/auth", StringComparison.OrdinalIgnoreCase))
                {
                    return RateLimitPartition.GetNoLimiter(httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
                }

                string? ip = httpContext.Connection.RemoteIpAddress?.ToString();
                return RateLimitPartition.GetFixedWindowLimiter(ip ?? string.Empty, _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 10
                });
            });
        });

        return services;
    }
}
