using EventManagement.Bookings.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Bookings.Infrastructure.Extensions
{
    /// <summary>
    /// Расширения для инициализации базы данных бронирований.
    /// </summary>
    public static class DatabaseInitializationExtensions
    {
        /// <summary>
        /// Применяет миграции к базе бронирований при старте приложения.
        /// </summary>
        public static IApplicationBuilder UseBookingsDatabaseInitialization(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            db.Database.Migrate();

            return app;
        }
    }
}
