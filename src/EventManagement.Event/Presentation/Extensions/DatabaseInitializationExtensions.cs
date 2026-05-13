using EventManagement.Events.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

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
