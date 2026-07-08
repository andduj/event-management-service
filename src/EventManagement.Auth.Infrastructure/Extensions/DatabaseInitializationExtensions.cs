using EventManagement.Auth.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagement.Auth.Infrastructure.Extensions
{
    /// <summary>
    /// Расширения для инициализации базы данных аутентификации.
    /// </summary>
    public static class DatabaseInitializationExtensions
    {
        /// <summary>
        /// Применяет миграции к базе auth при старте приложения.
        /// </summary>
        public static IApplicationBuilder UseAuthDatabaseInitialization(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            db.Database.Migrate();

            return app;
        }
    }
}
