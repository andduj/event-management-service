using AutoFixture;
using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Data.Repositories;
using EventManagement.Events.Api;
using EventManagement.Logging;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceFixture
    {
        public Mock<IEventsClient> EventsClient { get; }

        public IBookingRepository BookingRepository { get; }

        public BookingService BookingService { get; }

        public IFixture Fixture { get; }

        public EventDto CreateTestEvent(Guid eventId, int totalSeats, int? availableSeats = null)
        {
            var currentAvailableSeats = availableSeats ?? totalSeats;
            return Fixture
                .Build<EventDto>()
                .With(eventItem => eventItem.Id, eventId)
                .With(eventItem => eventItem.TotalSeats, totalSeats)
                .With(eventItem => eventItem.AvailableSeats, currentAvailableSeats)
                .Create();
        }

        public BookingServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            EventsClient = new Mock<IEventsClient>();

            BookingRepository = new InMemoryBookingRepository();

            Fixture = new Fixture();
            Fixture.Customize<EventDto>(composer => composer
                .FromFactory(() =>
                {
                    var startAt = DateTimeOffset.UtcNow;
                    return new EventDto
                    {
                        Id = Fixture.Create<Guid>(),
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = startAt.AddHours(1),
                        TotalSeats = 10,
                        AvailableSeats = 10,
                    };
                })
                .OmitAutoProperties());

            BookingService = new BookingService(
                BookingRepository,
                EventsClient.Object,
                mapper,
                new Mock<ILogger<BookingService>>().Object);
        }
    }
}
