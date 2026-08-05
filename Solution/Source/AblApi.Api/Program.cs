
using AblApi.Api;
using AblApi.Api.Startup;

namespace AblApi;

public class Program
{
    public static void Main(string[] args)
    {
        DotEnv.LoadEnvVariables();

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAppServices(builder.Configuration);
        builder.Services.AddEndpoints(builder.Configuration);

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        // builder.Services.AddOpenApi();

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // app.UseHttpsRedirection();

        // app.UseAuthorization();
        // app.UseAuthentication();

        app.MapControllers();

        app.Run();
    }
}
