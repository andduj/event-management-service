using AutoMapper;
using EventManagement.Application;
using EventManagement.Data.Repositories;

namespace EventService.Tests
{
    public class EventServiceFixture
    {
        public EventManagement.Application.Services.EventService EventService { get; }

        public EventServiceFixture()
        {
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
                .CreateMapper();

            EventService = new EventManagement.Application.Services.EventService(new InMemoryEventRepository(), mapper, new EventValidator());
        }
    }
}