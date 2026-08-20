using AblApi.Api.Startup;

namespace AblApi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "prod";

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddConfigProviders();

        builder.ConfigureLogging();

        builder.Services.AddAppServices(builder.Configuration)
                        .AddEndpoints(builder.Configuration)
                        .AddDatabaseServices(builder.Configuration)
                        .AddAuth(builder.Configuration);

        // TODO: configure OpenAPI (https://aka.ms/aspnet/openapi)
        // builder.Services.AddOpenApi();

        builder.Services.AddControllers()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            // TODO: log stack trace in development environment
            // app.UseExceptionHandler("/error-development");
        }
        else
        {
            // TODO: log error message only
            // app.UseExceptionHandler("/error");
        }

        // TODO: HTTPS communication with reverse proxy
        // app.UseHttpsRedirection();

        app.UseCors(policy => policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition"));

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
