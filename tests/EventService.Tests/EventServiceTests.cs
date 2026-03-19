using AutoFixture;
using EventManagement.Application.Filters;
using EventManagement.Application.Requests;
using EventManagement.Application.Services;
using EventManagement.Exceptions;
using FluentAssertions;
using FluentValidation;
using System;
using System.Collections.Generic;

namespace EventService.Tests
{
    public class EventServiceTests : IClassFixture<EventServiceFixture>
    {
        private readonly EventsService _eventsService;
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
            _eventsService = fixture.EventsService;
            _fixture = fixture.Fixture;
        }

        [Fact]
        public void Add_NewEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();

            var added = _eventsService.Add(addEventRequest);

            Assert.NotEqual(Guid.Empty, added.Id);
        }

        [Fact]
        public void Filter_GetAll_Success()
        {
            var paginatedResult = _eventsService.Filter(new EventFilter(), 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
        }

        [Fact]
        public void GetById_ExistingEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var added = _eventsService.Add(addEventRequest);

            var eventItem = _eventsService.GetById(added.Id);

            eventItem.Id.Should().Be(added.Id);
        }

        [Fact]
        public void Update_ExistingEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var added = _eventsService.Add(addEventRequest);
            var updateEventRequest = _fixture.Create<UpdateEventRequest>();

           _eventsService.Update(added.Id, updateEventRequest);

            var eventItem = _eventsService.GetById(added.Id);
            eventItem.Title.Should().Be(updateEventRequest.Title);
            eventItem.Description.Should().Be(updateEventRequest.Description);
            eventItem.StartAt.Should().Be(updateEventRequest.StartAt);
            eventItem.EndAt.Should().Be(updateEventRequest.EndAt);
        }

        [Fact]
        public void Delete_ExistingEvent_Success()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var added = _eventsService.Add(addEventRequest);

            _eventsService.Delete(added.Id);

            var action = ()=> _eventsService.GetById(added.Id);
            action.Should().Throw<EventNotFoundException>();
        }

        [Theory]
        [InlineData("Концерт")]
        [InlineData("Салют")]
        [InlineData("Фестиваль")]
        public void Filter_ByTitle_Success(string title)
        {
            var filter = new EventFilter
            {
                Title = title
            };

            var paginatedResult = _eventsService.Filter(filter, 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items
                .Should()
                .OnlyContain(eventItem => eventItem.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [MemberData(nameof(DateTimePeriods))]
        public void Filter_ByDate_Success(DateTime startAt, DateTime endAt)
        {
            var filter = new EventFilter
            {
                StartAt = startAt,
                EndAt = endAt
            };

            var paginatedResult = _eventsService.Filter(filter, 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items
                .Should()
                .OnlyContain(eventItem => eventItem.StartAt >= startAt && eventItem.EndAt <= endAt);
        }

        [Theory]
        [InlineData(1, 12, 12)]
        [InlineData(2, 12, 12)]
        [InlineData(3, 12, 12)]
        public void Filter_Pagination_Success(int page, int pageSize, int expectedCount)
        {
            var paginatedResult = _eventsService.Filter(new EventFilter(), page, pageSize);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items.Should().HaveCount(expectedCount);
        }

        [Theory]
        [MemberData(nameof(TitleAndDateTimePeriods))]
        public void Filter_Combined_Success(string title, DateTime startAt, DateTime endAt)
        {
            var filter = new EventFilter
            {
                Title = title,
                StartAt = startAt,
                EndAt = endAt
            };

            var paginatedResult = _eventsService.Filter(filter, 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
            paginatedResult.Items
                .Should()
                .OnlyContain(eventItem => eventItem.Title.Contains(title, StringComparison.OrdinalIgnoreCase) &&
                                            eventItem.StartAt >= startAt &&
                                            eventItem.EndAt <= endAt);
        }

        [Fact]
        public void GetById_NotExistingEvent_ShouldThrowEventNotFoundException()
        {
            var action = () => _eventsService.GetById(Guid.NewGuid());

            action.Should().Throw<EventNotFoundException>();
        }

        [Fact]
        public void Update_NotExistingEvent_ShouldThrowEventNotFoundException()
        {
            var updateEventRequest = _fixture.Create<UpdateEventRequest>();

            var action = () => _eventsService.Update(Guid.NewGuid(), updateEventRequest);

            action.Should().Throw<EventNotFoundException>();
        }

        [Fact]
        public void Add_WithoutTitle_ShouldThrowValidationException()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            addEventRequest.Title = string.Empty;

            var action = () => _eventsService.Add(addEventRequest);

            action.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Add_StartAtIsGreaterEndAt_ShouldThrowValidationException()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            addEventRequest.StartAt = DateTime.Now.AddHours(1);
            addEventRequest.EndAt = DateTime.Now;

            var action = () => _eventsService.Add(addEventRequest);

            action.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Update_StartAtIsGreaterEndAt_ShouldThrowValidationException()
        {
            var addEventRequest = _fixture.Create<AddEventRequest>();
            var addedEvent = _eventsService.Add(addEventRequest);
            var updateEventRequest = _fixture.Create<UpdateEventRequest>();
            updateEventRequest.StartAt = DateTime.Now.AddHours(1);
            updateEventRequest.EndAt = DateTime.Now;

            var action = () => _eventsService.Update(addedEvent.Id, updateEventRequest);

            action.Should().Throw<ValidationException>();
        }
    }
}