using AutoMapper;
using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Requests;
using EventModel = EventManagement.Events.Models.Event;

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
            CreateMap<AddEventRequest, EventModel>();
            CreateMap<UpdateEventRequest, EventModel>();
            CreateMap<EventModel, EventDto>();
        }
    }
}
