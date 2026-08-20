using AblApi.Api.Logging;
using AblApi.Core.AppSettings;
using Serilog;

namespace AblApi.Api.Startup;

public static class LoggingBuilderExtension
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        var environmentIdConfig = new EnvironmentIdAppSettings();
        builder.Configuration.GetSection(EnvironmentIdAppSettings.SectionName).Bind(environmentIdConfig);
        builder.Services.AddSingleton(environmentIdConfig);

        var elkConfig = new ElkAppSettings();
        builder.Configuration.GetSection(ElkAppSettings.SectionName).Bind(elkConfig);
        if (string.IsNullOrEmpty(elkConfig.ApiKey) || elkConfig.ApiKey == "APIKEY")
        {
            throw new InvalidOperationException("ELK API key is not configured.");
        }
        builder.Services.AddSingleton(elkConfig);

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("ASPNETCORE_ENVIRONMENT", context.HostingEnvironment.EnvironmentName);

            configuration.Enrich.With(new EnvironmentEnricher(environmentIdConfig));

            if (builder.Environment.IsDevelopment())
            {
                configuration
                    .WriteTo.Console()
                    .WriteTo.Debug();
            }
            else
            {
                configuration
                    .WriteTo.Elasticsearch(elkConfig);
            }
        });
    }
}
