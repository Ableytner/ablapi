using AblApi.Api.ExceptionHandling;
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

        builder.Services.AddOpenApi();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddControllers()
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        var app = builder.Build();

        // Unhandled exceptions in Development environment reply with a detailed error json response
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
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
