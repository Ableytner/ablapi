using AblApi.Api.Logging;
using AblApi.Core;
using AblApi.Core.AppSettings;
using Serilog;

namespace AblApi.Api.Startup;

public static class LoggingBuilderExtension
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        var elkConfig = new ElkAppSettings();
        builder.Configuration.GetSection(ElkAppSettings.SectionName).Bind(elkConfig);
        if (!EnvironmentHelper.IsDevelopment() && (string.IsNullOrEmpty(elkConfig.ApiKey) || elkConfig.ApiKey == "APIKEY"))
        {
            //throw new InvalidOperationException("ELK API key is not configured.");
        }
        builder.Services.AddSingleton(elkConfig);

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("DOTNET_ENVIRONMENT", context.HostingEnvironment.EnvironmentName);

            configuration.Enrich.With(new EnvironmentEnricher());

            if (EnvironmentHelper.IsDevelopment())
            {
                configuration
                    .WriteTo.Console()
                    .WriteTo.Debug();
            }
            else
            {
                configuration
                    .WriteTo.Console();
                /*configuration
                    .WriteTo.Elasticsearch(elkConfig);*/
            }
        });
    }
}
