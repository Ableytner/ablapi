using AblApi.Api.Logging;
using AblApi.Core.AppSettings;
using Serilog;

namespace AblApi.Api.Startup;

public static class LoggingBuilderExtension
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("ASPNETCORE_ENVIRONMENT", context.HostingEnvironment.EnvironmentName);

            EnvironmentIdAppSettings? envAppSettings = builder.Configuration
                .GetSection(EnvironmentIdAppSettings.SectionName)
                .Get<EnvironmentIdAppSettings>();

            if (envAppSettings is not null)
            {
                configuration.Enrich.With(new EnvironmentEnricher(envAppSettings));
            }

            if (builder.Environment.IsDevelopment())
            {
                configuration
                    .WriteTo.Console()
                    .WriteTo.Debug();
            }
            else
            {
                configuration
                    .WriteTo.Elasticsearch(context.Configuration);
            }
        });
    }
}
