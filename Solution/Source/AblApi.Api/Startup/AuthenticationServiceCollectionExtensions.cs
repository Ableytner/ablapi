using AblApi.Core.AppJwtToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AblApi.Api.Startup;

internal static class AuthenticationServiceCollectionExtensions
{
    // based on: https://codewithmukesh.com/blog/jwt-authentication-in-aspnet-core/
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwtConfig = new JwtAppSettings();
        config.GetSection(JwtAppSettings.SectionName).Bind(jwtConfig);
        if (string.IsNullOrEmpty(jwtConfig.Key) || jwtConfig.Key == "JWTKEY")
        {
            throw new InvalidOperationException("JWT key is not configured.");
        }
        services.AddSingleton(jwtConfig);

        services.AddTransient<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            // Keep the claim names exactly as they appear in the token (no surprise remapping).
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = JwtRegisteredClaimNames.Name,
                RoleClaimType = "role"
            };
        });

        services.AddAuthorization();

        return services;
    }
}
