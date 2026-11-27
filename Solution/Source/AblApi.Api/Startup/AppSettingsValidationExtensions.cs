using AblApi.Core.AppK8s;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AblApi.Api.Startup;

internal static class AppSettingsValidationExtensions
{
    public static IApplicationBuilder ValidateAppSettings(this IApplicationBuilder app)
    {
        var services = app.ApplicationServices;
        var settings = services.GetRequiredService<K8sAppSettings>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("K8sAppSettings");
        DeployTargetsValidator.ValidateAndLog(logger, settings.DeployTargets);

        return app;
    }
}
