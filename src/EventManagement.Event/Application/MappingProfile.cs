using AutoMapper;
using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Requests;
using EventManagement.Events.Models;

namespace EventManagement.Events.Application
{
    /// <summary>
    /// Конфигурация маппинга.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр профиля маппинга.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<AddEventRequest, Event>();
            CreateMap<UpdateEventRequest, Event>();
            CreateMap<Event, EventDto>();
        }
    }
}
