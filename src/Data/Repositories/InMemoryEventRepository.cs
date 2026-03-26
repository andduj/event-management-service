using EventManagement.Event.Application.DTOs;
using EventManagement.Event.Application.Filters;
using EventManagement.Event.Data.Interfaces;
using EventManagement.Event.Exceptions;
using LinqKit;
using EventModel = EventManagement.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventManagement.Event.Data.Repositories
{
    /// <summary>
    /// Репозиторий для работы с мероприятиями, реализующий хранение данных в оперативной памяти.
    /// </summary>
    public class InMemoryEventRepository : IEventRepository
    {
        private static readonly List<EventModel> _events;

        static InMemoryEventRepository() 
        {
            _events = EventsFactory.Create();
        }
        
        /// <inheritdoc/>
        public EventModel Add(EventModel newEvent)
        {
            _events.Add(newEvent);
            return newEvent;
        }

        /// <inheritdoc/>
        public void Delete(Guid id)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Мероприятие с id={id} не найдено.");
            }

            _events.Remove(eventItem);
        }

        /// <inheritdoc/>
        public PaginatedResult<EventModel> Filter(EventFilter eventFilter, int page, int pageSize)
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

            return new PaginatedResult<EventModel> 
            {
                Items = items, 
                Page = page, 
                PageSize = pageSize, 
                TotalItems = filteredCount, 
                TotalPages = totalPages
            };
        }

        /// <inheritdoc/>
        public EventModel GetById(Guid id)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Мероприятие с id={id} не найдено.");
            }
            return eventItem;
        }

        /// <inheritdoc/>
        public void Update(EventModel updatedEvent)
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
        }

        private static Expression<Func<EventModel, bool>> BuildPredicate(EventFilter filter)
        {
            var predicate = PredicateBuilder.New<EventModel>(true);

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
