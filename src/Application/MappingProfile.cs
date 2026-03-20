using AutoMapper;
using EventService.Application.DTOs;
using EventService.Application.Requests;
using EventService.Models;

namespace EventService.Application
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
