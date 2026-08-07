using AutoMapper;
using EventManagement.Events.Application;
using EventManagement.Events.Application.Caching;
using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Application.Requests;
using EventManagement.Events.Application.Services;
using EventManagement.Events.Application.Validators;
using EventManagement.Events.Domain.Models;
using EventManagement.Logging;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Tests
{
    /// <summary>
    /// Unit-тесты Cache-Aside и инвалидации кеша в <see cref="EventService"/>.
    /// </summary>
    public class EventServiceCacheTests
    {
        private readonly Mock<IEventRepository> _eventRepositoryMock = new();
        private readonly Mock<ICacheService> _cacheServiceMock = new();
        private readonly Mock<IEventLifecyclePublisher> _eventLifecyclePublisherMock = new();
        private readonly EventService _eventService;

        public EventServiceCacheTests()
        {
            IMapper mapper = new MapperConfiguration(configuration => configuration.AddProfile<MappingProfile>())
                .CreateMapper();

            _eventService = new EventService(
                _eventRepositoryMock.Object,
                _eventLifecyclePublisherMock.Object,
                _cacheServiceMock.Object,
                Options.Create(new RedisOptions
                {
                    EventTtlSeconds = 300,
                    Top10TtlSeconds = 60
                }),
                mapper,
                new AddEventRequestValidator(),
                new UpdateEventRequestValidator(),
                new Mock<ILogger<EventService>>().Object);
        }

        [Fact]
        public async Task GetEventByIdAsync_CacheHit_DoesNotCallRepository()
        {
            Guid eventId = Guid.NewGuid();
            string cacheKey = CacheKeys.EventById(eventId);
            var cachedEvent = new EventDto
            {
                Id = eventId,
                Title = "Cached event",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(1),
                TotalSeats = 100,
                AvailableSeats = 40
            };

            _cacheServiceMock
                .Setup(cache => cache.GetAsync<EventDto>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedEvent);

            EventDto result = await _eventService.GetEventByIdAsync(eventId);

            result.Should().BeEquivalentTo(cachedEvent);
            _eventRepositoryMock.Verify(
                repository => repository.GetEventByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _cacheServiceMock.Verify(
                cache => cache.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<EventDto>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetEventByIdAsync_CacheMiss_LoadsFromRepositoryAndSetsCache()
        {
            Guid eventId = Guid.NewGuid();
            string cacheKey = CacheKeys.EventById(eventId);
            Event eventItem = Event.Create(
                title: "DB event",
                startAt: DateTime.UtcNow,
                endAt: DateTime.UtcNow.AddHours(2),
                totalSeats: 50);
            eventItem.Id = eventId;

            _cacheServiceMock
                .Setup(cache => cache.GetAsync<EventDto>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventDto?)null);
            _eventRepositoryMock
                .Setup(repository => repository.GetEventByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventItem);

            EventDto result = await _eventService.GetEventByIdAsync(eventId);

            result.Id.Should().Be(eventId);
            result.Title.Should().Be("DB event");
            _eventRepositoryMock.Verify(
                repository => repository.GetEventByIdAsync(eventId, It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheServiceMock.Verify(
                cache => cache.SetAsync(
                    cacheKey,
                    It.Is<EventDto>(dto => dto.Id == eventId && dto.Title == "DB event"),
                    TimeSpan.FromSeconds(300),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTopPopularEventsAsync_CacheHit_DoesNotCallRepository()
        {
            var cachedTop = new List<EventDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Top event",
                    StartAt = DateTime.UtcNow,
                    EndAt = DateTime.UtcNow.AddHours(1),
                    TotalSeats = 10,
                    AvailableSeats = 1
                }
            };

            _cacheServiceMock
                .Setup(cache => cache.GetAsync<List<EventDto>>(CacheKeys.Top10Events, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedTop);

            IReadOnlyList<EventDto> result = await _eventService.GetTopPopularEventsAsync();

            result.Should().BeEquivalentTo(cachedTop);
            _eventRepositoryMock.Verify(
                repository => repository.GetTopPopularAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetTopPopularEventsAsync_CacheMiss_LoadsFromRepositoryAndSetsCache()
        {
            Event eventItem = Event.Create(
                title: "Popular",
                startAt: DateTime.UtcNow,
                endAt: DateTime.UtcNow.AddHours(1),
                totalSeats: 100);
            eventItem.AvailableSeats = 10;

            _cacheServiceMock
                .Setup(cache => cache.GetAsync<List<EventDto>>(CacheKeys.Top10Events, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<EventDto>?)null);
            _eventRepositoryMock
                .Setup(repository => repository.GetTopPopularAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Event> { eventItem });

            IReadOnlyList<EventDto> result = await _eventService.GetTopPopularEventsAsync();

            result.Should().ContainSingle(dto => dto.Id == eventItem.Id);
            _eventRepositoryMock.Verify(
                repository => repository.GetTopPopularAsync(10, It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheServiceMock.Verify(
                cache => cache.SetAsync(
                    CacheKeys.Top10Events,
                    It.Is<List<EventDto>>(list => list.Count == 1 && list[0].Id == eventItem.Id),
                    TimeSpan.FromSeconds(60),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateEventAsync_AfterDatabaseUpdate_InvalidatesEventCache()
        {
            Guid eventId = Guid.NewGuid();
            Event eventItem = Event.Create(
                title: "Old title",
                startAt: DateTime.UtcNow,
                endAt: DateTime.UtcNow.AddHours(1),
                totalSeats: 20);
            eventItem.Id = eventId;

            var updateRequest = new UpdateEventRequest
            {
                Title = "New title",
                Description = "Updated",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(2),
                AvailableSeats = 15
            };

            _eventRepositoryMock
                .Setup(repository => repository.GetEventByIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventItem);
            _eventRepositoryMock
                .Setup(repository => repository.UpdateEventAsync(eventItem))
                .Returns(Task.CompletedTask);

            await _eventService.UpdateEventAsync(eventId, updateRequest);

            _eventRepositoryMock.Verify(repository => repository.UpdateEventAsync(eventItem), Times.Once);
            _cacheServiceMock.Verify(
                cache => cache.RemoveAsync(CacheKeys.EventById(eventId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteEventAsync_AfterDatabaseDelete_InvalidatesEventCache()
        {
            Guid eventId = Guid.NewGuid();
            _eventRepositoryMock
                .Setup(repository => repository.DeleteEventAsync(eventId))
                .Returns(Task.CompletedTask);

            await _eventService.DeleteEventAsync(eventId);

            _eventRepositoryMock.Verify(repository => repository.DeleteEventAsync(eventId), Times.Once);
            _cacheServiceMock.Verify(
                cache => cache.RemoveAsync(CacheKeys.EventById(eventId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TryReserveSeats_WhenReserved_InvalidatesEventCache()
        {
            Guid eventId = Guid.NewGuid();
            _eventRepositoryMock
                .Setup(repository => repository.TryReserveSeats(eventId, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            bool wasReserved = await _eventService.TryReserveSeats(eventId, 2);

            wasReserved.Should().BeTrue();
            _cacheServiceMock.Verify(
                cache => cache.RemoveAsync(CacheKeys.EventById(eventId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TryReserveSeats_WhenNotReserved_DoesNotInvalidateEventCache()
        {
            Guid eventId = Guid.NewGuid();
            _eventRepositoryMock
                .Setup(repository => repository.TryReserveSeats(eventId, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            bool wasReserved = await _eventService.TryReserveSeats(eventId, 2);

            wasReserved.Should().BeFalse();
            _cacheServiceMock.Verify(
                cache => cache.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ReleaseSeats_AfterDatabaseUpdate_InvalidatesEventCache()
        {
            Guid eventId = Guid.NewGuid();
            _eventRepositoryMock
                .Setup(repository => repository.ReleaseSeats(eventId, 1, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _eventService.ReleaseSeats(eventId, 1);

            _cacheServiceMock.Verify(
                cache => cache.RemoveAsync(CacheKeys.EventById(eventId), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
