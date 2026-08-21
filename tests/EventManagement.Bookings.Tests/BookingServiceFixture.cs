using AutoFixture;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.Data.Repositories;
using EventManagement.Bookings.Infrastructure.DataAccess;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceFixture
    {
        public IServiceProvider ServiceProvider { get; }

        public IServiceScope Scope { get; }

        public IBookingRepository BookingRepository { get; }

        public IBookableEventRepository BookableEventRepository { get; }

        public IBookingService BookingService { get; }

        public Guid TestUserId { get; }

        public IFixture Fixture { get; }

        public BookingServiceFixture()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            services.AddSingleton(new Mock<ILogger<BookingService>>().Object);
            services.AddSingleton(new Mock<IBookingConfirmedPublisher>().Object);
            services.AddDbContext<BookingsDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookableEventRepository, BookableEventRepository>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddAutoMapper(typeof(MappingProfile));

            ServiceProvider = services.BuildServiceProvider();
            TestUserId = Guid.NewGuid();

            Scope = ServiceProvider.CreateScope();
            BookingRepository = Scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            BookableEventRepository = Scope.ServiceProvider.GetRequiredService<IBookableEventRepository>();

            Fixture = new Fixture();
            Fixture.Customize<Booking>(composer => composer
                .FromFactory(() => Booking.Create(Guid.NewGuid(), Guid.NewGuid()))
                .OmitAutoProperties());
            Fixture.Customize<BookableEvent>(composer => composer
                .FromFactory(() => BookableEvent.Create(
                    Guid.NewGuid(),
                    "Test event",
                    "Description",
                    DateTime.UtcNow.AddDays(1),
                    DateTime.UtcNow.AddDays(1).AddHours(2),
                    10,
                    10))
                .OmitAutoProperties());

            BookingService = Scope.ServiceProvider.GetRequiredService<IBookingService>();
        }

        public async Task<BookableEvent> SeedBookableEventAsync(
            Guid? eventId = null,
            int availableSeats = 10,
            DateTime? startAt = null)
        {
            var bookableEvent = BookableEvent.Create(
                eventId ?? Guid.NewGuid(),
                "Test event",
                "Description",
                startAt ?? DateTime.UtcNow.AddDays(1),
                (startAt ?? DateTime.UtcNow.AddDays(1)).AddHours(2),
                availableSeats,
                availableSeats);

            await BookableEventRepository.UpsertAsync(bookableEvent);
            return bookableEvent;
        }
    }
}
