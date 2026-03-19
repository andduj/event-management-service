using EventManagement.Presentation.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Presentation
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
        /// <returns>Модифицированная коллекция сервисов для дальнейшей настройки.</returns>
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSwaggerDocumentation();

            return services;
        }
    }
}
