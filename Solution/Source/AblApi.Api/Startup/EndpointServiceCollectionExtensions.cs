namespace AblApi.Api.Startup;

internal static class EndpointServiceCollectionExtensions
{
	public static IServiceCollection AddEndpoints(this IServiceCollection services, IConfiguration config)
	{
		// Routing
		services.AddControllers();

		services.AddCors(opt =>
		{
			opt.AddPolicy(name: "CorsPolicy", build =>
			{
				build.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod().WithExposedHeaders("*");
			});
		});

		return services;
	}
}
