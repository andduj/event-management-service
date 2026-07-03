using AutoFixture;
using EventManagement.Events.Application;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Application.Requests;
using EventManagement.Events.Application.Services;
using EventManagement.Events.Application.Validators;
using EventManagement.Events.Infrastructure.Data.Repositories;
using EventManagement.Events.Infrastructure.DataAccess;
using EventManagement.Events.Domain.Models;
using EventManagement.Logging;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventManagement.Events.Tests
{
    public class EventServiceFixture
    {
        private const int MinTotalSeats = 1;
        private const int MaxTotalSeats = 5000;

        public IServiceScope Scope { get; }

        public IServiceProvider ServiceProvider { get; }

        public IFixture Fixture { get; }

        public EventServiceFixture()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<EventsDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IValidator<AddEventRequest>, AddEventRequestValidator>();
            services.AddScoped<IValidator<UpdateEventRequest>, UpdateEventRequestValidator>();
            services.AddSingleton(new Mock<IEventLifecyclePublisher>().Object);
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddSingleton(new Mock<ILogger<EventService>>().Object);
            ServiceProvider = services.BuildServiceProvider();
            Scope = ServiceProvider.CreateScope();
            SeedEvents();

            Fixture = new Fixture();

            Fixture.Customize<AddEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var totalSeats = Random.Shared.Next(MinTotalSeats, MaxTotalSeats);
                    var startAt = DateTime.UtcNow;

                    return new AddEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = startAt.AddHours(1),
                        TotalSeats = totalSeats
                    };
                })
                .OmitAutoProperties());

            Fixture.Customize<UpdateEventRequest>(composer => composer
                .FromFactory(() =>
                {
                    var startAt = DateTime.UtcNow;

                    return new UpdateEventRequest
                    {
                        Title = Fixture.Create<string>(),
                        Description = Fixture.Create<string>(),
                        StartAt = startAt,
                        EndAt = startAt.AddHours(1),
                        AvailableSeats = 0
                    };
                })
                .OmitAutoProperties());
        }

        public IEventService EventService => Scope.ServiceProvider.GetRequiredService<IEventService>();

        private void SeedEvents()
        {
            using var scope = ServiceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventsDbContext>();

            if (db.Events.Any())
            {
                return;
            }

            var titles = new[] { "Концерт", "Салют", "Фестиваль" };
            var periods = new[]
            {
                (new DateTime(2026, 1, 1), new DateTime(2026, 1, 30)),
                (new DateTime(2026, 5, 1), new DateTime(2026, 5, 30)),
                (new DateTime(2026, 10, 1), new DateTime(2026, 10, 30))
            };

            var seededEvents = new List<Event>();
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < titles.Length; j++)
                {
                    var (startAt, endAt) = periods[j];
                    seededEvents.Add(Event.Create(
                        title: titles[j],
                        startAt: startAt.AddHours(i),
                        endAt: endAt.AddHours(i),
                        totalSeats: 100,
                        description: $"Seed event {i}-{j}"));
                }
            }

            db.Events.AddRange(seededEvents);
            db.SaveChanges();
        }
    }
}
