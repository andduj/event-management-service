using AutoFixture;
using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Logging;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingProcessingServiceFixture
    {
        public Mock<IBookingRepository> BookingRepository { get; }

        public Mock<IBookableEventRepository> BookableEventRepository { get; }

        public Mock<IBookingConfirmedPublisher> BookingConfirmedPublisher { get; }

        public BookingProcessingService BookingProcessingService { get; }

        public BookingService BookingService { get; }

        public IFixture Fixture { get; }

        public BookingProcessingServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            BookingRepository = new Mock<IBookingRepository>();
            BookableEventRepository = new Mock<IBookableEventRepository>();
            BookingConfirmedPublisher = new Mock<IBookingConfirmedPublisher>();

            BookingProcessingService = new BookingProcessingService(
                BookingRepository.Object,
                BookableEventRepository.Object,
                BookingConfirmedPublisher.Object,
                new Mock<ILogger<BookingProcessingService>>().Object);

            BookingService = new BookingService(
                BookingRepository.Object,
                BookableEventRepository.Object,
                BookingConfirmedPublisher.Object,
                mapper,
                new Mock<ILogger<BookingService>>().Object);

            Fixture = new Fixture();
            Fixture.Customize<Booking>(composer => composer
                .FromFactory(() => Booking.Create(Guid.NewGuid(), Guid.NewGuid()))
                .OmitAutoProperties());
        }
    }
}
