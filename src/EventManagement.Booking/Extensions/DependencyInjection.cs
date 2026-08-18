using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace EventManagement.Bookings.Extensions
{
    /// <summary>
    /// Класс для регистрации зависимостей уровня представления в DI контейнере.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы уровня представления в указанный <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">Коллекция сервисов (контейнер DI).</param>
        /// <param name="configuration">Конфигурация приложения.</param>
        /// <returns>Модифицированная коллекция сервисов для дальнейшей настройки.</returns>
        public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerDocumentation();
            services.AddJwtAuthentication(configuration);
            services.AddFrontendCors(configuration);

            return services;
        }
    }
}
