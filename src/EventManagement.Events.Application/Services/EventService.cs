using AutoMapper;
using EventManagement.Events.Application.Caching;
using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Application.Requests;
using EventManagement.Events.Domain.Models;
using EventManagement.Logging;
using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Application.Services
{
    /// <summary>
    /// Сервис для работы с мероприятиями.
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventLifecyclePublisher _eventLifecyclePublisher;
        private readonly ICacheService _cacheService;
        private readonly RedisOptions _redisOptions;
        private readonly IMapper _mapper;
        private readonly IValidator<AddEventRequest> _addEventRequestValidator;
        private readonly IValidator<UpdateEventRequest> _updateEventRequestValidator;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IEventRepository eventRepository,
            IEventLifecyclePublisher eventLifecyclePublisher,
            ICacheService cacheService,
            IOptions<RedisOptions> redisOptions,
            IMapper mapper,
            IValidator<AddEventRequest> addEventRequestValidator,
            IValidator<UpdateEventRequest> updateEventRequestValidator,
            ILogger<EventService> logger)
        {
            _eventRepository = eventRepository;
            _eventLifecyclePublisher = eventLifecyclePublisher;
            _cacheService = cacheService;
            _redisOptions = redisOptions.Value;
            _mapper = mapper;
            _addEventRequestValidator = addEventRequestValidator;
            _updateEventRequestValidator = updateEventRequestValidator;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EventDto> CreateEventAsync(AddEventRequest addEventRequest)
        {
            _logger.Info("Создание нового мероприятия.");
            DefaultValidatorExtensions.ValidateAndThrow(_addEventRequestValidator, addEventRequest);
            var newEvent = Event.Create(
                addEventRequest.Title ?? string.Empty,
                addEventRequest.StartAt,
                addEventRequest.EndAt,
                addEventRequest.TotalSeats!.Value,
                addEventRequest.Description);
            var addedEvent = await _eventRepository.CreateEventAsync(newEvent);
            await InvalidateEventCacheAsync(addedEvent.Id);
            await _eventLifecyclePublisher.PublishCreatedAsync(addedEvent);
            _logger.Info("Мероприятие успешно создано. Id={0}", addedEvent.Id);
            return _mapper.Map<EventDto>(addedEvent);
        }

        /// <inheritdoc/>
        public async Task DeleteEventAsync(Guid id)
        {
            _logger.Info("Удаление мероприятия. Id={0}", id);
            await _eventRepository.DeleteEventAsync(id);
            await InvalidateEventCacheAsync(id);
            await _eventLifecyclePublisher.PublishDeletedAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> Exists(Guid id, CancellationToken cancellationToken = default)
        {
            return await _eventRepository.Exists(id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PaginatedResult<EventDto>> FilterAsync(EventFilter eventFilter, int page, int pageSize)
        {
            _logger.Debug("Получение списка мероприятий. Page={0}, PageSize={1}", page, pageSize);
            var paginatedResult = await _eventRepository.FilterAsync(eventFilter, page, pageSize);
            var events = paginatedResult.Items
                .Select(_mapper.Map<EventDto>)
                .ToList();

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
            string cacheKey = CacheKeys.EventById(id);
            var cachedEvent = await _cacheService.GetAsync<EventDto>(cacheKey);
            if (cachedEvent is not null)
            {
                _logger.Debug("Мероприятие получено из кеша. Id={0}", id);
                return cachedEvent;
            }

            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            var eventDto = _mapper.Map<EventDto>(eventItem);
            await _cacheService.SetAsync(
                cacheKey,
                eventDto,
                TimeSpan.FromSeconds(_redisOptions.EventTtlSeconds));
            return eventDto;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<EventDto>> GetTopPopularEventsAsync(CancellationToken cancellationToken = default)
        {
            _logger.Debug("Получение топ-10 популярных мероприятий.");
            var cachedTop = await _cacheService.GetAsync<List<EventDto>>(
                CacheKeys.Top10Events,
                cancellationToken);
            if (cachedTop is not null)
            {
                _logger.Debug("Топ-10 мероприятий получен из кеша.");
                return cachedTop;
            }

            var topEvents = await _eventRepository.GetTopPopularAsync(10, cancellationToken);
            var topDtos = topEvents
                .Select(_mapper.Map<EventDto>)
                .ToList();
            await _cacheService.SetAsync(
                CacheKeys.Top10Events,
                topDtos,
                TimeSpan.FromSeconds(_redisOptions.Top10TtlSeconds),
                cancellationToken);
            return topDtos;
        }

        /// <inheritdoc/>
        public async Task<bool> TryReserveSeats(Guid id, int count)
        {
            _logger.Debug("Попытка резервирования {0} мест для мероприятия Id={1}", count, id);
            bool wasReserved = await _eventRepository.TryReserveSeats(id, count);
            if (wasReserved)
            {
                await InvalidateEventCacheAsync(id);
            }

            _logger.Debug("Результат резервирования для мероприятия Id={0}: {1}", id, wasReserved);
            return wasReserved;
        }

        /// <inheritdoc/>
        public async Task ReleaseSeats(Guid id, int count)
        {
            _logger.Debug("Освобождение {0} мест для мероприятия Id={1}", count, id);
            await _eventRepository.ReleaseSeats(id, count);
            await InvalidateEventCacheAsync(id);
            _logger.Debug("Места успешно освобождены для мероприятия Id={0}", id);
        }

        /// <inheritdoc/>
        public async Task UpdateEventAsync(Guid id, UpdateEventRequest updateEventRequest)
        {
            _logger.Info("Обновление мероприятия. Id={0}", id);
            DefaultValidatorExtensions.ValidateAndThrow(_updateEventRequestValidator, updateEventRequest);
            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            eventItem.Title = updateEventRequest.Title;
            eventItem.Description = updateEventRequest.Description;
            eventItem.StartAt = updateEventRequest.StartAt;
            eventItem.EndAt = updateEventRequest.EndAt;
            eventItem.SetAvailableSeats(updateEventRequest.AvailableSeats);
            await _eventRepository.UpdateEventAsync(eventItem);
            await InvalidateEventCacheAsync(id);
            await _eventLifecyclePublisher.PublishUpdatedAsync(eventItem);
        }

        private Task InvalidateEventCacheAsync(Guid eventId)
        {
            return _cacheService.RemoveAsync(CacheKeys.EventById(eventId));
        }
    }
}
