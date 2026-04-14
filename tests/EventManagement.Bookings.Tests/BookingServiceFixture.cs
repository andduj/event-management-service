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

        public BookingServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            EventsClient = new Mock<IEventsClient>();
            BookingRepository = new InMemoryBookingRepository();

            BookingService = new BookingService(
                BookingRepository,
                EventsClient.Object,
                mapper,
                new Mock<ILogger<BookingService>>().Object);
        }
    }
}
