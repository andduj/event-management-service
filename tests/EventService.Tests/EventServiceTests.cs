using AutoFixture;
using EventManagement.Application.Requests;
using FluentAssertions;
using System;
using EventManagement.Application.Filters;

namespace EventService.Tests
{
    public class EventServiceTests : IClassFixture<EventServiceFixture>
    {
        private readonly EventManagement.Application.Services.EventService _eventService;

        public EventServiceTests(EventServiceFixture fixture)
        {
            _eventService = fixture.EventService;
        }

        [Fact]
        public void Add_NewEvent_Success()
        {
            var fixture = new Fixture();
            var addEventRequest = fixture.Create<AddEventRequest>();

            var added = _eventService.Add(addEventRequest);

            Assert.NotEqual(Guid.Empty, added.Id);
        }

        [Fact]
        public void Filter_GetAll_Success()
        {
            var paginatedResult = _eventService.Filter(new EventFilter(), 1, int.MaxValue);

            paginatedResult.Items.Should().NotBeEmpty();
        }
    }
}