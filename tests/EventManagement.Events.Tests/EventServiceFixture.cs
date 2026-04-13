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
        private const int MinHours = 1;
        private const int MaxHours = 5;
        private const int Year = 2026;
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
                    var startAt = GetRandomDateTimeUtcInYear(Year);
                    var endAt = startAt.AddHours(Random.Shared.Next(MinHours, MaxHours + 1));
                    var totalSeats = Random.Shared.Next(MinTotalSeats, MaxTotalSeats);

                    return new AddEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = endAt,
                        TotalSeats = totalSeats
                    };
                })
                .OmitAutoProperties());

            Fixture.Customize<UpdateEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var startAt = GetRandomDateTimeUtcInYear(Year);
                    var endAt = startAt.AddHours(Random.Shared.Next(MinHours, MaxHours + 1));
                    var totalSeats = Random.Shared.Next(MinTotalSeats, MaxTotalSeats);
                    var availableSeats = Random.Shared.Next(0, totalSeats + 1);

                    return new UpdateEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = endAt,
                        AvailableSeats = availableSeats
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
