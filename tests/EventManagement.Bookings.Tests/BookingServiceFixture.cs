using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Repositories;
using EventManagement.Events.Api;
using EventManagement.Logging;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceFixture
    {
        public Mock<IEventsClient> EventsClient { get; }

        public BookingService BookingService { get; }

        public BookingServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            EventsClient = new Mock<IEventsClient>();

            BookingService = new BookingService(
                new InMemoryBookingRepository(),
                EventsClient.Object,
                mapper,
                new Mock<ILogger<BookingService>>().Object);
        }
    }
}
