using EventManagement.Events.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Events.DataAccess
{
    /// <summary>
    /// Сидер тестовых мероприятий.
    /// </summary>
    internal static class EventsDataSeeder
    {
        /// <summary>
        /// Добавляет тестовые данные, если таблица событий пустая.
        /// </summary>
        public static async Task SeedIfNeededAsync(EventsDbContext db, CancellationToken cancellationToken = default)
        {
            if (await db.Events.AnyAsync(cancellationToken))
            {
                return;
            }

            var events = EventsFactory.Create();
            db.Events.AddRange(events);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
