using AutoFixture;
using AutoMapper;
using EventManagement.Events.Application;
using EventManagement.Events.Application.Requests;
using EventManagement.Events.Application.Services;
using EventManagement.Events.Application.Validators;
using EventManagement.Events.Data.Repositories;
using EventManagement.Logging;
using Moq;
using System;

namespace EventManagement.Events.Tests
{
    public class EventServiceFixture
    {
        private const int MinTotalSeats = 1;
        private const int MaxTotalSeats = 5000;

        public EventService EventService { get; }

        public IFixture Fixture { get; }

        public EventServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            EventService = new EventService(
                new InMemoryEventRepository(),
                mapper,
                new AddEventRequestValidator(),
                new UpdateEventRequestValidator(),
                new Mock<ILogger<EventService>>().Object);

            Fixture = new Fixture();

            Fixture.Customize<AddEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var totalSeats = Random.Shared.Next(MinTotalSeats, MaxTotalSeats);
                    var startAt = DateTime.UtcNow;

                    return new AddEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = startAt.AddHours(1),
                        TotalSeats = totalSeats
                    };
                })
                .OmitAutoProperties());

            Fixture.Customize<UpdateEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var totalSeats = Random.Shared.Next(MinTotalSeats, MaxTotalSeats);
                    var availableSeats = Random.Shared.Next(0, totalSeats + 1);
                    var startAt = DateTime.UtcNow;

                    return new UpdateEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = startAt.AddHours(1),
                        AvailableSeats = availableSeats
                    };
                })
                .OmitAutoProperties());
        }
    }
}
