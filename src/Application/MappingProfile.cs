using AutoMapper;
using EventManagement.Application.DTOs;
using EventManagement.Application.Requests;
using EventManagement.Models;

namespace EventManagement.Application
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
