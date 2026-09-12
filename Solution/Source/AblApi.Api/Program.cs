using AblApi.Api.ExceptionHandling;
using AblApi.Api.Startup;
using AblApi.Core;

namespace AblApi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddConfigProviders();

        builder.ConfigureLogging();

        builder.Services.AddAppServices(builder.Configuration)
                        .AddEndpoints(builder.Configuration)
                        .AddDatabaseServices(builder.Configuration)
                        .AddAuth(builder.Configuration);

        builder.Services.AddOpenApi();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddControllers()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        builder.Services.AddRateLimiting();

        var app = builder.Build();

        // Unhandled exceptions in Development environment reply with a detailed error json response
        app.UseExceptionHandler();

        if (EnvironmentHelper.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "AblApi v1");
                options.RoutePrefix = "swagger";
            });
        }

        // TODO: HTTPS communication with reverse proxy
        // app.UseHttpsRedirection();

        app.UseCors();

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
