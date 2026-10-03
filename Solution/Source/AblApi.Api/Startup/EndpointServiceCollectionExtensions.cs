using AblApi.Core.AppSettings;

namespace AblApi.Api.Startup;

internal static class EndpointServiceCollectionExtensions
{
	public static IServiceCollection AddEndpoints(this IServiceCollection services, IConfiguration config)
	{
		services.AddControllers();

		var corsConfig = new CorsAppSettings();
		config.GetSection(CorsAppSettings.SectionName).Bind(corsConfig);

		services.AddCors(opt =>
		{
			opt.AddDefaultPolicy(build =>
			{
				var origins = corsConfig.AllowedOriginsArray;

				var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
				loggerFactory.CreateLogger("AblApi.Api.Startup.EndpointServiceCollectionExtensions")
					         .LogInformation("Allowed CORS origins: [{Origins}]", string.Join(", ", origins));

				if (origins.Length > 0)
				{
					build.WithOrigins(origins)
						.AllowAnyHeader()
						.AllowAnyMethod()
						.AllowCredentials()
						.WithExposedHeaders("Content-Disposition");
				}
			});
		});

		return services;
	}
}
