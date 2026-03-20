using AutoFixture;
using AutoMapper;
using EventService.Application;
using EventService.Application.Requests;
using EventService.Application.Services;
using EventService.Data.Repositories;
using EventService.Logging;
using Moq;
using System;

namespace EventService.Tests
{
    public class EventServiceFixture
    {
        private const int MinHours = 1;
        private const int MaxHours = 5;
        private const int Year = 2026;

        public EventsService EventsService { get; }

        public IFixture Fixture { get; }

        public EventServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            EventsService = new EventsService(
                new InMemoryEventRepository(),
                mapper,
                new EventValidator(),
                new Mock<ILogger<EventsService>>().Object);

            Fixture = new Fixture();

            Fixture.Customize<AddEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var startAt = GetRandomDateTimeUtcInYear(Year);
                    var endAt = startAt.AddHours(Random.Shared.Next(MinHours, MaxHours + 1));

                    return new AddEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = endAt
                    };
                })
                .OmitAutoProperties());

            Fixture.Customize<UpdateEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var startAt = GetRandomDateTimeUtcInYear(Year);
                    var endAt = startAt.AddHours(Random.Shared.Next(MinHours, MaxHours + 1));

                    return new UpdateEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = endAt
                    };
                })
                .OmitAutoProperties());
        }

        private static DateTime GetRandomDateTimeUtcInYear(int year)
        {
            var daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;
            var dayOfYear = Random.Shared.Next(1, daysInYear + 1);
            var date = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(dayOfYear - 1);

            var hour = Random.Shared.Next(0, 24);
            var minute = Random.Shared.Next(0, 60);
            var second = Random.Shared.Next(0, 60);

            return date.AddHours(hour).AddMinutes(minute).AddSeconds(second);
        }
    }
}