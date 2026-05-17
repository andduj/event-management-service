using EventManagement.Bookings.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Bookings.Presentation.Extensions
{
    /// <summary>
    /// Расширения для инициализации базы данных.
    /// </summary>
    public static class DatabaseInitializationExtensions
    {
        /// <summary>
        /// Инициализирует базу данных бронирований при старте приложения.
        /// </summary>
        /// <param name="app">Построитель конвейера обработки запросов приложения.</param>
        /// <returns>Построитель конвейера для дальнейшей настройки.</returns>
        public static IApplicationBuilder UseDatabaseInitialization(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            db.Database.Migrate();

            return app;
        }
    }
}
