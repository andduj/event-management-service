using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Репозиторий локальной проекции мероприятий на базе EF Core.
    /// </summary>
    public class BookableEventRepository : IBookableEventRepository
    {
        private readonly BookingsDbContext _context;

        /// <summary>
        /// Инициализирует репозиторий проекций мероприятий.
        /// </summary>
        /// <param name="context">Контекст данных бронирований.</param>
        public BookableEventRepository(BookingsDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<BookableEvent?> TryGetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.BookableEvents
                .FirstOrDefaultAsync(bookableEvent => bookableEvent.Id == eventId, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return _context.BookableEvents.AnyAsync(bookableEvent => bookableEvent.Id == eventId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpsertAsync(BookableEvent bookableEvent, CancellationToken cancellationToken = default)
        {
            var existing = await _context.BookableEvents
                .FirstOrDefaultAsync(item => item.Id == bookableEvent.Id, cancellationToken);

            if (existing == null)
            {
                _context.BookableEvents.Add(bookableEvent);
            }
            else
            {
                int mergedAvailableSeats = Math.Min(existing.AvailableSeats, bookableEvent.AvailableSeats);
                mergedAvailableSeats = Math.Min(mergedAvailableSeats, bookableEvent.TotalSeats);
                existing.Sync(
                    bookableEvent.Title,
                    bookableEvent.Description,
                    bookableEvent.StartAt,
                    bookableEvent.EndAt,
                    bookableEvent.TotalSeats,
                    mergedAvailableSeats);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var existing = await _context.BookableEvents
                .FirstOrDefaultAsync(bookableEvent => bookableEvent.Id == eventId, cancellationToken);

            if (existing == null)
            {
                return;
            }

            _context.BookableEvents.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> TryReserveSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default)
        {
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                var bookableEvent = await TryGetByIdAsync(eventId, cancellationToken);
                if (bookableEvent == null)
                {
                    return false;
                }

                if (!bookableEvent.TryReserveSeats(count))
                {
                    return false;
                }

                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            int rowsAffected = await _context.BookableEvents
                .Where(bookableEvent => bookableEvent.Id == eventId && bookableEvent.AvailableSeats >= count)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        bookableEvent => bookableEvent.AvailableSeats,
                        bookableEvent => bookableEvent.AvailableSeats - count),
                    cancellationToken);

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task ReleaseSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default)
        {
            var bookableEvent = await TryGetByIdAsync(eventId, cancellationToken);
            if (bookableEvent == null)
            {
                return;
            }

            bookableEvent.ReleaseSeats(count);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
