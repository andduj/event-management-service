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
        /// Регистрирует параметры Redis и singleton-соединение <see cref="IConnectionMultiplexer"/>.
        /// </summary>
        /// <param name="services">Коллекция сервисов (контейнер DI).</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Модифицированная коллекция сервисов.</returns>
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                RedisOptions redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
                    ?? new RedisOptions();
                ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                configurationOptions.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            return services;
        }
    }
}
