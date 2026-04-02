using AutoMapper;
using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Models;

namespace EventManagement.Bookings.Application
{
    /// <summary>
    /// Конфигурация маппинга для домена бронирований.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Инициализирует новый экземпляр профиля маппинга.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Booking, BookingDto>();
            CreateMap<Booking, BookingInfo>();
        }
    }
}
