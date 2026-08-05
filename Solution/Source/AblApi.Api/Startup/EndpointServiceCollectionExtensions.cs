using Asp.Versioning;
using Asp.Versioning.Routing;

namespace AblApi.Api.Startup;

internal static class EndpointServiceCollectionExtensions
{
	public static IServiceCollection AddEndpoints(this IServiceCollection services, IConfiguration config)
	{
		// Routing
		services.AddControllers();

		services.AddApiVersioning(options =>
		{
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.DefaultApiVersion = new ApiVersion(1, 0);
			options.ReportApiVersions = true;
		});

		// Register the 'apiVersion' route constraint  
		services.Configure<RouteOptions>(options =>
		{
			options.ConstraintMap["apiVersion"] = typeof(ApiVersionRouteConstraint);
		});

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
