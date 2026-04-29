using EventManagement.Events.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Events.Presentation.Extensions
{
    /// <summary>
    /// Расширения для инициализации базы данных.
    /// </summary>
    public static class DatabaseInitializationExtensions
    {
        /// <summary>
        /// Инициализирует базу данных мероприятий при старте приложения.
        /// </summary>
        /// <param name="app">Построитель конвейера обработки запросов приложения.</param>
        /// <returns>Построитель конвейера для дальнейшей настройки.</returns>
        public static IApplicationBuilder UseDatabaseInitialization(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
            db.Database.EnsureCreated();

            return app;
        }
    }
}
