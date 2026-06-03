using AutoFixture;
using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Events.Api;
using EventManagement.Logging;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingProcessingServiceFixture
    {
        public Mock<IBookingRepository> BookingRepository { get; }

        public Mock<IEventsClient> EventsClient { get; }

        public BookingProcessingService BookingProcessingService { get; }

        public BookingService BookingService { get; }

        public IFixture Fixture { get; }

        public BookingProcessingServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            BookingRepository = new Mock<IBookingRepository>();
            EventsClient = new Mock<IEventsClient>();

            BookingProcessingService = new BookingProcessingService(
                BookingRepository.Object,
                EventsClient.Object,
                new Mock<ILogger<BookingProcessingService>>().Object);

            BookingService = new BookingService(
                BookingRepository.Object,
                EventsClient.Object,
                mapper,
                new Mock<ILogger<BookingService>>().Object);

            Fixture = new Fixture();
            Fixture.Customize<Booking>(composer => composer
                .FromFactory(() => Booking.Create(Guid.NewGuid()))
                .OmitAutoProperties());
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
        }

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
    }
}
