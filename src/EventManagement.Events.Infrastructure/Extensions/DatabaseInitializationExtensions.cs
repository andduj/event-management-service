using EventManagement.Events.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace EventManagement.Events.Infrastructure.Extensions
{
    /// <summary>
    /// Расширения для инициализации базы данных мероприятий.
    /// </summary>
    public static class DatabaseInitializationExtensions
    {
        /// <summary>
        /// Применяет миграции и при необходимости заполняет БД тестовыми данными.
        /// </summary>
        /// <param name="app">Построитель конвейера обработки запросов приложения.</param>
        /// <returns>Построитель конвейера для дальнейшей настройки.</returns>
        public static IApplicationBuilder UseEventsDatabaseInitialization(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            db.Database.Migrate();
            bool seedOnStartup = configuration.GetValue<bool>("DatabaseInitialization:SeedOnStartup");
            if (seedOnStartup)
            {
                EventsDataSeeder.SeedIfNeededAsync(db, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }

            return app;
        }
    }
}
