using AutoMapper;
using EventManagement.Application.DTOs;
using EventManagement.Application.Filters;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Requests;
using EventManagement.Data.Interfaces;
using EventManagement.Models;

namespace EventManagement.Application.Services
{
    /// <summary>
    /// Сервис для работы с мероприятиями
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public EventService(IEventRepository eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public EventDto Add(AddEventRequest addEventRequest)
        {
            var newEvent = _mapper.Map<Event>(addEventRequest);
            newEvent.Id = Guid.NewGuid();
            var addedEvent = _eventRepository.Add(newEvent);
            return _mapper.Map<EventDto>(addedEvent);
        }

        /// <inheritdoc/>
        public void Delete(Guid id)
        {
            _eventRepository.Delete(id);
        }

        /// <inheritdoc/>
        public PaginatedResult<EventDto> Filter(EventFilter eventFilter, int page, int pageSize)
        {
            var paginatedResult = _eventRepository.Filter(eventFilter, page, pageSize);
            var events = paginatedResult.Items
                .Select(_mapper.Map<EventDto>)
                .ToList()
                .AsReadOnly();

            return new PaginatedResult<EventDto>
            {
                Items = events,
                Page = paginatedResult.Page,
                PageSize = paginatedResult.PageSize,
                TotalItems = paginatedResult.TotalItems,
                TotalPages = paginatedResult.TotalPages
            };
        }

        /// <inheritdoc/>
        public EventDto GetById(Guid id)
        {
            var eventItem = _eventRepository.GetById(id);
            return _mapper.Map<EventDto>(eventItem);
        }

        /// <inheritdoc/>
        public void Update(Guid id, UpdateEventRequest updateEventRequest)
        {
            var updatedEvent = _mapper.Map<Event>(updateEventRequest);
            updatedEvent.Id = id;
            _eventRepository.Update(updatedEvent);
        }
    }
}
