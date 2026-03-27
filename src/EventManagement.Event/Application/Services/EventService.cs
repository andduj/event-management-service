using AutoMapper;
using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Application.Requests;
using EventManagement.Events.Data.Interfaces;
using EventModel = EventManagement.Events.Models.Event;
using FluentValidation;
using System;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.Logging;

namespace EventManagement.Events.Application.Services
{
    /// <summary>
    /// Сервис для работы с мероприятиями.
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<EventModel> _validator;
        private readonly ILogger<EventService> _logger;

        public EventService(IEventRepository eventRepository, IMapper mapper, IValidator<EventModel> validator, ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EventDto> CreateEventAsync(AddEventRequest addEventRequest)
        {
            _logger.Info("Создание нового мероприятия.");
            var newEvent = _mapper.Map<EventModel>(addEventRequest);
            _validator.ValidateAndThrow(newEvent);
            newEvent.Id = Guid.NewGuid();
            var addedEvent = await _eventRepository.CreateEventAsync(newEvent);
            _logger.Info("Мероприятие успешно создано. Id={0}", addedEvent.Id);
            return _mapper.Map<EventDto>(addedEvent);
        }

        /// <inheritdoc/>
        public async Task DeleteEventAsync(Guid id)
        {
            _logger.Info("Удаление мероприятия. Id={0}", id);
            await _eventRepository.DeleteEventAsync(id);
        }

        /// <inheritdoc/>
        public async Task<PaginatedResult<EventDto>> FilterAsync(EventFilter eventFilter, int page, int pageSize)
        {
            _logger.Debug("Получение списка мероприятий. Page={0}, PageSize={1}", page, pageSize);
            var paginatedResult = await _eventRepository.FilterAsync(eventFilter, page, pageSize);
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
        public async Task<EventDto> GetEventByIdAsync(Guid id)
        {
            _logger.Debug("Получение мероприятия по Id={0}", id);
            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            return _mapper.Map<EventDto>(eventItem);
        }

        /// <inheritdoc/>
        public async Task UpdateEventAsync(Guid id, UpdateEventRequest updateEventRequest)
        {
            _logger.Info("Обновление мероприятия. Id={0}", id);
            var updatedEvent = _mapper.Map<EventModel>(updateEventRequest);
            _validator.ValidateAndThrow(updatedEvent);
            updatedEvent.Id = id;
            await _eventRepository.UpdateEventAsync(updatedEvent);
        }
    }
}
