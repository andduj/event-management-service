using AutoFixture;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.Data.Repositories;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventManagement.Bookings.Tests
{
    public class BookingServiceFixture
    {
        public IServiceProvider ServiceProvider { get; }

        public IServiceScope Scope { get; }

        public Mock<IEventsGateway> EventsGateway { get; }

        public IBookingRepository BookingRepository { get; }

        public IBookingService BookingService { get; }

        public Guid TestUserId { get; }

        public IFixture Fixture { get; }

        public BookingServiceFixture()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            EventsGateway = new Mock<IEventsGateway>();
            services.AddSingleton(EventsGateway.Object);
            services.AddSingleton(new Mock<ILogger<BookingService>>().Object);
            services.AddDbContext<BookingsDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddAutoMapper(typeof(MappingProfile));

            ServiceProvider = services.BuildServiceProvider();

            using (var seedScope = ServiceProvider.CreateScope())
            {
                var context = seedScope.ServiceProvider.GetRequiredService<BookingsDbContext>();
                var userRepository = seedScope.ServiceProvider.GetRequiredService<IUserRepository>();
                var user = User.Create("booking-service-test-user", "HASH", UserRole.User);
                userRepository.CreateAsync(user).GetAwaiter().GetResult();
                TestUserId = user.Id;
            }

            Scope = ServiceProvider.CreateScope();
            BookingRepository = Scope.ServiceProvider.GetRequiredService<IBookingRepository>();

            Fixture = new Fixture();
            Fixture.Customize<Booking>(composer => composer
                .FromFactory(() => Booking.Create(Guid.NewGuid(), Guid.NewGuid()))
                .OmitAutoProperties());

            BookingService = Scope.ServiceProvider.GetRequiredService<IBookingService>();
        }
    }
}
