using EventManagement.Auth.Application.Interfaces;
using EventManagement.Auth.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Auth.Application
{
    /// <summary>
    /// Класс для регистрации зависимостей уровня приложения в DI контейнере.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы уровня приложения в указанный <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Коллекция сервисов (контейнер DI).</param>
        /// <returns>Модифицированная коллекция сервисов для дальнейшей настройки.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
