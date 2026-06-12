using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Data.Repositories;
using EventManagement.Bookings.DataAccess;
using EventManagement.Events.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

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
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не настроена.");
            services.AddDbContext<BookingsDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddSingleton(typeof(Logging.ILogger<>), typeof(Logging.Logger<>));
            services.Configure<BookingProcessingOptions>(configuration.GetSection(BookingProcessingOptions.SectionName));
            services.AddHostedService<BookingBackgroundService>();

            string eventsBaseUrl = configuration["ExternalServices:EventsBaseUrl"] ?? "https://localhost:7216";
            services.AddHttpClient("EventsApi", client =>
            {
                client.BaseAddress = new Uri(eventsBaseUrl);
            });
            services.AddScoped<IEventsClient>(serviceProvider =>
            {
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient("EventsApi");
                return new EventsClient(eventsBaseUrl, httpClient);
            });
            services.AddScoped<IEventsGateway, EventsGateway>();

            return services;
        }
    }
}
