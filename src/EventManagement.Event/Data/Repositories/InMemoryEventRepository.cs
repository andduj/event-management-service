using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Data.Interfaces;
using EventManagement.Events.Exceptions;
using EventManagement.Events.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Data.Repositories
{
    /// <summary>
    /// Репозиторий для работы с мероприятиями, реализующий хранение данных в оперативной памяти.
    /// </summary>
    public class InMemoryEventRepository : IEventRepository
    {
        private static readonly List<Event> _events;

        static InMemoryEventRepository() 
        {
            _events = EventsFactory.Create();
        }
        
        /// <inheritdoc/>
        public async Task<Event> CreateEventAsync(Event newEvent)
        {
            _events.Add(newEvent);
            return await Task.FromResult(newEvent);
        }

        /// <inheritdoc/>
        public async Task DeleteEventAsync(Guid id)
        {
            var eventItem = await GetEventByIdAsync(id);
            _events.Remove(eventItem);
        }

        /// <inheritdoc/>
        public async Task<PaginatedResult<Event>> FilterAsync(EventFilter eventFilter, int page, int pageSize)
        {
            var predicate = BuildPredicate(eventFilter);
            var query = _events
                .Where(predicate.Compile());

            int filteredCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .AsReadOnly();

            int totalPages = (int)Math.Ceiling((double)filteredCount / pageSize);

            var result = new PaginatedResult<Event> 
            {
                Items = items, 
                Page = page, 
                PageSize = pageSize, 
                TotalItems = filteredCount, 
                TotalPages = totalPages
            };

            return await Task.FromResult(result);
        }

        /// <inheritdoc/>
        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Мероприятие с id={id} не найдено.");
            }

            return await Task.FromResult(eventItem);
        }

        /// <inheritdoc/>
        public async Task UpdateEventAsync(Event updatedEvent)
        {
            var eventItem = await GetEventByIdAsync(updatedEvent.Id);
            eventItem.Title = updatedEvent.Title;
            eventItem.Description = updatedEvent.Description;
            eventItem.StartAt = updatedEvent.StartAt;
            eventItem.EndAt = updatedEvent.EndAt;
        }

        /// <inheritdoc/>
        public async Task<bool> Exists(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool exists = _events.Any(e => e.Id == id);
            return await Task.FromResult(exists);
        }

        public async Task<bool> TryReserveSeats(Guid id, int count = 1)
        {
            var eventItem = await GetEventByIdAsync(id);            
            return eventItem.TryReserveSeats(count);
        }

        public async Task ReleaseSeats(Guid id, int count = 1)
        {
            var eventItem = await GetEventByIdAsync(id);
            eventItem.ReleaseSeats();
        }

        private static Expression<Func<Event, bool>> BuildPredicate(EventFilter filter)
        {
            var predicate = PredicateBuilder.New<Event>(true);

            if (!string.IsNullOrEmpty(filter.Title))
            {
                predicate = predicate.And(e => e.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.StartAt.HasValue)
            {
                predicate = predicate.And(e => e.StartAt >= filter.StartAt);
            }

            if (filter.EndAt.HasValue)
            {
                predicate = predicate.And(e => e.EndAt <= filter.EndAt);
            }

            return predicate;
        }
    }
}
