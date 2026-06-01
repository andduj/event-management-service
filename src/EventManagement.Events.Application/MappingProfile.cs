using AutoMapper;
using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Domain.Models;

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
            CreateMap<Event, EventDto>();
        }
    }
}
