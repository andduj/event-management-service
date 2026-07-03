using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Infrastructure.Data.Repositories;
using EventManagement.Events.Infrastructure.DataAccess;
using EventManagement.Events.Infrastructure.Kafka;
using EventManagement.Events.Infrastructure.Messaging;
using EventManagement.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EventManagement.Events.Infrastructure.Extensions
{
    /// <summary>
    /// Регистрация зависимостей уровня инфраструктуры в DI-контейнере.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы уровня инфраструктуры в указанный <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Коллекция сервисов (контейнер DI).</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Модифицированная коллекция сервисов.</returns>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Строка подключения 'DefaultConnection' не настроена.");
            services.AddDbContext<EventsDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
            services.AddSingleton<IEventLifecyclePublisher, KafkaEventLifecyclePublisher>();

            return services;
        }
    }
}
