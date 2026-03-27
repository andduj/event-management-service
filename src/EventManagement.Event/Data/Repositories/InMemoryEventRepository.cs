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
        public Task<Event> CreateEventAsync(Event newEvent)
        {
            _events.Add(newEvent);
            return Task.FromResult(newEvent);
        }

        /// <inheritdoc/>
        public Task DeleteEventAsync(Guid id)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Мероприятие с id={id} не найдено.");
            }

            _events.Remove(eventItem);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<PaginatedResult<Event>> FilterAsync(EventFilter eventFilter, int page, int pageSize)
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

            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public Task<Event> GetEventByIdAsync(Guid id)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Мероприятие с id={id} не найдено.");
            }

            return Task.FromResult(eventItem);
        }

        /// <inheritdoc/>
        public Task UpdateEventAsync(Event updatedEvent)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == updatedEvent.Id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Мероприятие с id={updatedEvent.Id} не найдено.");
            }

            eventItem.Title = updatedEvent.Title;
            eventItem.Description = updatedEvent.Description;
            eventItem.StartAt = updatedEvent.StartAt;
            eventItem.EndAt = updatedEvent.EndAt;

            return Task.CompletedTask;
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
