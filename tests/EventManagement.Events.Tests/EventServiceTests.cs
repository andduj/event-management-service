using AutoFixture;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Application.Requests;
using EventManagement.Events.Application.Services;
using EventManagement.Events.Exceptions;
using FluentAssertions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Events.Tests
{
    public class EventServiceTests : IClassFixture<EventServiceFixture>
    {
        private readonly EventService _eventService;
        private readonly IFixture _fixture;

        public static IEnumerable<object[]> DateTimePeriods()
        {
            return
            [
                [new DateTime(2026, 1, 1), new DateTime(2026, 1, 30)],
                [new DateTime(2026, 5, 1), new DateTime(2026, 5, 30)],
                [new DateTime(2026, 10, 1), new DateTime(2026, 10, 30)]
            ];
        }

        public static IEnumerable<object[]> TitleAndDateTimePeriods()
        {
            return
            [
                ["Концерт", new DateTime(2026, 1, 1), new DateTime(2026, 1, 30)],
                ["Салют", new DateTime(2026, 5, 1), new DateTime(2026, 5, 30)],
                ["Фестиваль", new DateTime(2026, 10, 1), new DateTime(2026, 10, 30)]
            ];
        }

        public EventServiceTests(EventServiceFixture fixture)
        {
            _eventService = fixture.EventService;
            _fixture = fixture.Fixture;
        }

        [Fact]
        public async Task Add_NewEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();

            var added = await _eventService.CreateEventAsync(addEventRequest);

            Assert.NotEqual(Guid.Empty, added.Id);
        }

        [Fact]
        public async Task Filter_GetAll_Success()
        {
            var paginatedResult = await _eventService.FilterAsync(new EventFilter(), 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_ExistingEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var added = await _eventService.CreateEventAsync(addEventRequest);

            var eventItem = await _eventService.GetEventByIdAsync(added.Id);

            eventItem.Id.Should().Be(added.Id);
        }

        [Fact]
        public async Task Update_ExistingEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var added = await _eventService.CreateEventAsync(addEventRequest);
            var updateEventRequest = _fixture.Create<UpdateEventRequest>();

           await _eventService.UpdateEventAsync(added.Id, updateEventRequest);

            var eventItem = await _eventService.GetEventByIdAsync(added.Id);
            eventItem.Title.Should().Be(updateEventRequest.Title);
            eventItem.Description.Should().Be(updateEventRequest.Description);
            eventItem.StartAt.Should().Be(updateEventRequest.StartAt);
            eventItem.EndAt.Should().Be(updateEventRequest.EndAt);
        }

        [Fact]
        public async Task Delete_ExistingEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var added = await _eventService.CreateEventAsync(addEventRequest);

            await _eventService.DeleteEventAsync(added.Id);

            Func<Task> action = ()=> _eventService.GetEventByIdAsync(added.Id);
            await action.Should().ThrowAsync<EventNotFoundException>();
        }

        [Theory]
        [InlineData("Концерт")]
        [InlineData("Салют")]
        [InlineData("Фестиваль")]
        public async Task Filter_ByTitle_Success(string title)
        {
            var filter = new EventFilter
            {
                Title = title
            };

            var paginatedResult = await _eventService.FilterAsync(filter, 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items
                .Should()
                .OnlyContain(eventItem => eventItem.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [MemberData(nameof(DateTimePeriods))]
        public async Task Filter_ByDate_Success(DateTime startAt, DateTime endAt)
        {
            var filter = new EventFilter
            {
                StartAt = startAt,
                EndAt = endAt
            };

            var paginatedResult = await _eventService.FilterAsync(filter, 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items
                .Should()
                .OnlyContain(eventItem => eventItem.StartAt >= startAt && eventItem.EndAt <= endAt);
        }

        [Theory]
        [InlineData(1, 12, 12)]
        [InlineData(2, 12, 12)]
        [InlineData(3, 12, 12)]
        public async Task Filter_Pagination_Success(int page, int pageSize, int expectedCount)
        {
            var paginatedResult = await _eventService.FilterAsync(new EventFilter(), page, pageSize);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items.Should().HaveCount(expectedCount);
        }

        [Theory]
        [MemberData(nameof(TitleAndDateTimePeriods))]
        public async Task Filter_Combined_Success(string title, DateTime startAt, DateTime endAt)
        {
            var filter = new EventFilter
            {
                Title = title,
                StartAt = startAt,
                EndAt = endAt
            };

            var paginatedResult = await _eventService.FilterAsync(filter, 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items
                .Should()
                .OnlyContain(eventItem => eventItem.Title.Contains(title, StringComparison.OrdinalIgnoreCase) &&
                                            eventItem.StartAt >= startAt &&
                                            eventItem.EndAt <= endAt);
        }

        [Fact]
        public async Task GetById_NotExistingEvent_ShouldThrowEventNotFoundException()
        {
            Func<Task> action = () => _eventService.GetEventByIdAsync(Guid.NewGuid());

            await action.Should().ThrowAsync<EventNotFoundException>();
        }

        [Fact]
        public async Task Update_NotExistingEvent_ShouldThrowEventNotFoundException()
        {
            var updateEventRequest = _fixture.Create<UpdateEventRequest>();

            Func<Task> action = () => _eventService.UpdateEventAsync(Guid.NewGuid(), updateEventRequest);

            await action.Should().ThrowAsync<EventNotFoundException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public async Task Add_InvalidTitle_ShouldThrowValidationException(string? title)
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            addEventRequest.Title = title;

            Func<Task> action = () => _eventService.CreateEventAsync(addEventRequest);

            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Add_StartAtIsGreaterEndAt_ShouldThrowValidationException()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            addEventRequest.StartAt = DateTime.Now.AddHours(1);
            addEventRequest.EndAt = DateTime.Now;

            Func<Task> action = () => _eventService.CreateEventAsync(addEventRequest);

            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Update_StartAtIsGreaterEndAt_ShouldThrowValidationException()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var addedEvent = await _eventService.CreateEventAsync(addEventRequest);
            var updateEventRequest = _fixture.Create<UpdateEventRequest>();
            updateEventRequest.StartAt = DateTime.Now.AddHours(1);
            updateEventRequest.EndAt = DateTime.Now;

            Func<Task> action = () => _eventService.UpdateEventAsync(addedEvent.Id, updateEventRequest);

            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Filter_PageOutOfRange_ShouldNoExceptions()
        {
            var paginatedResult = await _eventService.FilterAsync(new EventFilter(), 100, 100);

            paginatedResult.Items.Should().BeEmpty();
        }
    }
}
