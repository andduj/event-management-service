using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Infrastructure.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EventManagement.Events.Infrastructure.Extensions
{
    /// <summary>
    /// Расширения для регистрации Redis в DI-контейнере.
    /// </summary>
    public static class RedisRegistrationExtensions
    {
        /// <summary>
        /// Регистрирует параметры Redis, singleton-соединение и реализацию кеша.
        /// </summary>
        /// <param name="services">Коллекция сервисов (контейнер DI).</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Модифицированная коллекция сервисов.</returns>
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
                    ?? new RedisOptions();
                var configurationOptions = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                configurationOptions.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configurationOptions);
            });
            services.AddSingleton<ICacheService, RedisCacheService>();

            return services;
        }
    }
}
