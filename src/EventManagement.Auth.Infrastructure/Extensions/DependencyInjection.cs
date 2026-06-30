using EventManagement.Auth.Application.Interfaces;
using EventManagement.Auth.Application.Options;
using EventManagement.Auth.Infrastructure.Data.Repositories;
using EventManagement.Auth.Infrastructure.DataAccess;
using EventManagement.Auth.Infrastructure.Security;
using EventManagement.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventManagement.Auth.Infrastructure.Extensions
{
    /// <summary>
    /// Регистрация зависимостей уровня инфраструктуры в DI-контейнере.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы уровня инфраструктуры в указанный <see cref="IServiceCollection"/>.
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не настроена.");

            services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            return services;
        }
    }
}
