using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Infrastructure.Data.Repositories;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Events.Api;
using EventManagement.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace EventManagement.Bookings.Infrastructure.Extensions
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
            services.AddDbContext<BookingsDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
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
                if (httpClient.BaseAddress is null)
                {
                    throw new InvalidOperationException("HttpClient 'EventsApi' не имеет BaseAddress.");
                }

                string baseUrl = httpClient.BaseAddress.ToString().TrimEnd('/');
                return new EventsClient(baseUrl, httpClient);
            });
            services.AddScoped<IEventsGateway, EventsGateway>();

            return services;
        }
    }
}
