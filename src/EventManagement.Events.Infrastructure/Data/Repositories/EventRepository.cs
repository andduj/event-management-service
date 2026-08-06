using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Domain.Exceptions;
using EventManagement.Events.Domain.Models;
using EventManagement.Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Репозиторий мероприятий на базе EF Core.
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly EventsDbContext _context;

        /// <summary>
        /// Инициализирует репозиторий мероприятий.
        /// </summary>
        /// <param name="context">Контекст данных мероприятий.</param>
        public EventRepository(EventsDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<Event> CreateEventAsync(Event newEvent)
        {
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }

        /// <inheritdoc />
        public async Task DeleteEventAsync(Guid id)
        {
            var eventItem = await GetEventByIdAsync(id);
            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<PaginatedResult<Event>> FilterAsync(EventFilter eventFilter, int page, int pageSize)
        {
            var query = _context.Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(eventFilter.Title))
            {
                if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                {
                    var titleFilter = eventFilter.Title.ToLowerInvariant();
                    query = query.Where(e => e.Title.ToLowerInvariant().Contains(titleFilter));
                }
                else
                {
                    query = query.Where(e => EF.Functions.ILike(e.Title, $"%{eventFilter.Title}%"));
                }
            }

            if (eventFilter.StartAt.HasValue)
            {
                query = query.Where(e => e.StartAt >= eventFilter.StartAt.Value);
            }

            if (eventFilter.EndAt.HasValue)
            {
                query = query.Where(e => e.EndAt <= eventFilter.EndAt.Value);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Event>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            };
        }

        /// <inheritdoc />
        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Мероприятие с id={id} не найдено.");
            }

            return eventItem;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Event>> GetTopPopularAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.TotalSeats > 0)
                .OrderByDescending(e => (double)(e.TotalSeats - e.AvailableSeats) / e.TotalSeats)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateEventAsync(Event updatedEvent)
        {
            _context.Events.Update(updatedEvent);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> Exists(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Events.AnyAsync(e => e.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> TryReserveSeats(Guid id, int count)
        {
            var eventItem = await GetEventByIdAsync(id);
            bool wasReserved = eventItem.TryReserveSeats(count);
            if (wasReserved)
            {
                await _context.SaveChangesAsync();
            }

            return wasReserved;
        }

        /// <inheritdoc />
        public async Task ReleaseSeats(Guid id, int count)
        {
            var eventItem = await GetEventByIdAsync(id);
            eventItem.ReleaseSeats(count);
            await _context.SaveChangesAsync();
        }
    }
}
