using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace EventManagement.Events.Extensions
{
    /// <summary>
    /// Регистрация JWT-аутентификации.
    /// </summary>
    public static class JwtAuthenticationExtensions
    {
        private const string JwtSectionName = "Jwt";

        /// <summary>
        /// Добавляет JWT-аутентификацию и авторизацию.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            string secret = configuration[$"{JwtSectionName}:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret не настроен.");
            string issuer = configuration[$"{JwtSectionName}:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer не настроен.");
            string audience = configuration[$"{JwtSectionName}:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience не настроен.");

            byte[] key = Encoding.UTF8.GetBytes(secret);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    };
                });

            services.AddAuthorization();
            return services;
        }
    }
}
