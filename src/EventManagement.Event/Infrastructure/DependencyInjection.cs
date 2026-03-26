using EventManagement.Event.Data.Interfaces;
using EventManagement.Event.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Event.Infrastructure
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
            services.AddSingleton(typeof(EventManagement.Event.Logging.ILogger<>), typeof(EventManagement.Event.Logging.Logger<>));

            return services;
        }
    }
}
