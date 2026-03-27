using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Data.Repositories;
using EventManagement.Events.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddSingleton(typeof(Logging.ILogger<>), typeof(Logging.Logger<>));
            services.AddHostedService<BookingBackgroundService>();

            services.AddHttpClient<IEventsClient, EventsClient>(client =>
            {
                var baseUrl = configuration["ExternalServices:EventsBaseUrl"] ?? "https://localhost:7216";
                client.BaseAddress = new Uri(baseUrl);
            });

            return services;
        }
    }
}
