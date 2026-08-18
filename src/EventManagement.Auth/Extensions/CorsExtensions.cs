using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventManagement.Auth.Extensions
{
    /// <summary>
    /// Расширения для настройки CORS.
    /// </summary>
    public static class CorsExtensions
    {
        public const string FrontendPolicyName = "Frontend";

        /// <summary>
        /// Регистрирует CORS-политику для фронтенда.
        /// Источники берутся из секции конфигурации <c>Cors:Origins</c>.
        /// </summary>
        public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy(FrontendPolicyName, policy =>
                {
                    if (origins.Length == 0)
                    {
                        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                        return;
                    }

                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
