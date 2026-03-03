using EventManagement.Application.Interfaces;
using EventManagement.Application.Services;

namespace EventManagement.Application
{
    /// <summary>
    /// Регистрация зависимостей уровня приложения в DI контейнере.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы уровня приложения в указанный <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Коллекция сервисов (контейнер DI).</param>
        /// <returns>Модифицированная коллекция сервисов для дальнейшей настройки.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddAutoMapper(typeof(Program));
            return services;
        }
    }
}
