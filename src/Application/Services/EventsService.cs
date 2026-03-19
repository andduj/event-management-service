using AutoMapper;
using EventService.Application.DTOs;
using EventService.Application.Filters;
using EventService.Application.Interfaces;
using EventService.Application.Requests;
using EventService.Data.Interfaces;
using EventService.Models;
using EventService.Logging;
using FluentValidation;
using System;
using System.Linq;

namespace EventService.Application.Services
{
    /// <summary>
    /// Сервис для работы с мероприятиями.
    /// </summary>
    public class EventsService : IEventsService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<Event> _validator;
        private readonly ILogger<EventsService> _logger;

        public EventsService(IEventRepository eventRepository, IMapper mapper, IValidator<Event> validator, ILogger<EventsService> logger)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        /// <inheritdoc/>
        public EventDto Add(AddEventRequest addEventRequest)
        {
            _logger.Info("Создание нового мероприятия.");
            var newEvent = _mapper.Map<Event>(addEventRequest);
            _validator.ValidateAndThrow(newEvent);
            newEvent.Id = Guid.NewGuid();
            var addedEvent = _eventRepository.Add(newEvent);
            _logger.Info("Мероприятие успешно создано. Id={0}", addedEvent.Id);
            return _mapper.Map<EventDto>(addedEvent);
        }

        /// <inheritdoc/>
        public void Delete(Guid id)
        {
            _logger.Info("Удаление мероприятия. Id={0}", id);
            _eventRepository.Delete(id);
        }

        /// <inheritdoc/>
        public PaginatedResult<EventDto> Filter(EventFilter eventFilter, int page, int pageSize)
        {
            _logger.Debug("Получение списка мероприятий. Page={0}, PageSize={1}", page, pageSize);
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
            _logger.Debug("Получение мероприятия по Id={0}", id);
            var eventItem = _eventRepository.GetById(id);
            return _mapper.Map<EventDto>(eventItem);
        }

        /// <inheritdoc/>
        public void Update(Guid id, UpdateEventRequest updateEventRequest)
        {
            _logger.Info("Обновление мероприятия. Id={0}", id);
            var updatedEvent = _mapper.Map<Event>(updateEventRequest);
            _validator.ValidateAndThrow(updatedEvent);
            updatedEvent.Id = id;
            _eventRepository.Update(updatedEvent);
        }
    }
}
