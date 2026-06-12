using AutoFixture;
using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Logging;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingProcessingServiceFixture
    {
        public Mock<IBookingRepository> BookingRepository { get; }

        public Mock<IEventsGateway> EventsGateway { get; }

        public BookingProcessingService BookingProcessingService { get; }

        public BookingService BookingService { get; }

        public IFixture Fixture { get; }

        public BookingProcessingServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            BookingRepository = new Mock<IBookingRepository>();
            EventsGateway = new Mock<IEventsGateway>();

            BookingProcessingService = new BookingProcessingService(
                BookingRepository.Object,
                EventsGateway.Object,
                new Mock<ILogger<BookingProcessingService>>().Object);

            BookingService = new BookingService(
                BookingRepository.Object,
                EventsGateway.Object,
                mapper,
                new Mock<ILogger<BookingService>>().Object);

            Fixture = new Fixture();
            Fixture.Customize<Booking>(composer => composer
                .FromFactory(() => Booking.Create(Guid.NewGuid()))
                .OmitAutoProperties());
        }
    }
}
