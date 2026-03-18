using AutoFixture;
using AutoMapper;
using EventManagement.Application;
using EventManagement.Application.Requests;
using EventManagement.Data.Repositories;
using System;

namespace EventService.Tests
{
    public class EventServiceFixture
    {
        private const int MinHours = 1;
        private const int MaxHours = 5;

        public EventManagement.Application.Services.EventService EventService { get; }

        public IFixture Fixture { get; }

        public EventServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            EventService = new EventManagement.Application.Services.EventService(new InMemoryEventRepository(), mapper, new EventValidator());

            Fixture = new Fixture();

            Fixture.Customize<AddEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var startAt = GetRandomStartDate();
                    var endAt = startAt.AddHours(Random.Shared.Next(MinHours, MaxHours));

                    return new AddEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = endAt
                    };
                }));

            Fixture.Customize<UpdateEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var startAt = GetRandomStartDate();
                    var endAt = startAt.AddHours(Random.Shared.Next(MinHours, MaxHours));

                    return new UpdateEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = endAt
                    };
                }));
        }

        private DateTime GetRandomStartDate()
        {
            return new DateTime(2026,
                Random.Shared.Next(1, 13),
                Random.Shared.Next(1, 28),
                Random.Shared.Next(0, 24),
                Random.Shared.Next(0, 60),
                0);
        }
    }
}