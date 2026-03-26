using AutoMapper;
using EventManagement.Event.Application.DTOs;
using EventManagement.Event.Application.Requests;
using EventModel = EventManagement.Models.Event;

namespace EventManagement.Event.Application
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
