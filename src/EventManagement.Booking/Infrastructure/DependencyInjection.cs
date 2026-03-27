using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Data.Repositories;
using EventManagement.Events.Data.Interfaces;
using EventManagement.Events.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Bookings.Infrastructure
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
            services.AddScoped<IBookingRepository, InMemoryBookingRepository>();
            services.AddScoped<IEventRepository, InMemoryEventRepository>();
            services.AddSingleton(typeof(Logging.ILogger<>), typeof(Logging.Logger<>));
            services.AddHostedService<BookingBackgroundService>();
            return services;
        }
    }
}
