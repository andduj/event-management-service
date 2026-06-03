using AutoFixture;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Infrastructure.Data.Repositories;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Events.Api;
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

        public Mock<IEventsClient> EventsClient { get; }

        public IBookingRepository BookingRepository { get; }

        public IBookingService BookingService { get; }

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
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();

            EventsClient = new Mock<IEventsClient>();
            services.AddSingleton(EventsClient.Object);
            services.AddSingleton(new Mock<ILogger<BookingService>>().Object);
            services.AddDbContext<BookingsDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddAutoMapper(typeof(MappingProfile));

            ServiceProvider = services.BuildServiceProvider();
            Scope = ServiceProvider.CreateScope();
            BookingRepository = Scope.ServiceProvider.GetRequiredService<IBookingRepository>();

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

            BookingService = Scope.ServiceProvider.GetRequiredService<IBookingService>();
        }
    }
}
