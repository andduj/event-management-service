using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Options;
using EventManagement.Bookings.Infrastructure.Data.Repositories;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Bookings.Infrastructure.Kafka;
using EventManagement.Bookings.Infrastructure.Messaging;
using EventManagement.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            services.AddScoped<IBookableEventRepository, BookableEventRepository>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.Configure<BookingProcessingOptions>(configuration.GetSection(BookingProcessingOptions.SectionName));
            services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
            services.AddHostedService<BookingBackgroundService>();
            services.AddHostedService<EventLifecycleConsumer>();

            return services;
        }
    }
}
