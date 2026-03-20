using EventService.Data.Interfaces;
using EventService.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Infrastructure
{
    /// <summary>
    /// Класс для регистрации зависимостей уровня инфраструктуры в DI контейнере.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы уровня инфраструктуры в указанный <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Коллекция сервисов (контейнер DI).</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Модифицированная коллекция сервисов.</returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEventRepository, InMemoryEventRepository>();
            services.AddSingleton(typeof(EventService.Logging.ILogger<>), typeof(EventService.Logging.Logger<>));

            return services;
        }
    }
}
