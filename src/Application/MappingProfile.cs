using AutoMapper;
using EventManagement.Application.DTOs;
using EventManagement.Application.Requests;
using EventManagement.Models;

namespace EventManagement.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AddEventRequest, Event>();
            CreateMap<UpdateEventRequest, Event>();
            CreateMap<Event, EventDto>();
        }
    }
}
