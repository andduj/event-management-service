using EventManagement.Events.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Events.Infrastructure.Extensions
{
    /// <summary>
    /// Расширения для инициализации базы данных мероприятий.
    /// </summary>
    public static class DatabaseInitializationExtensions
    {
        /// <summary>
        /// Применяет миграции к базе мероприятий при старте приложения.
        /// </summary>
        public static IApplicationBuilder UseEventsDatabaseInitialization(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();
            db.Database.Migrate();

            return app;
        }
    }
}
