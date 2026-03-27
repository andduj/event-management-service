using EventManagement.Bookings.Models;
using EventManagement.Bookings.Application.DTOs;
using System.Threading.Tasks;
using System;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с бронированиями.
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// Создает бронь для указанного мероприятия.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <returns>Созданная бронь.</returns>
        Task<Booking> CreateBookingAsync(Guid eventId);

        /// <summary>
        /// Получает бронь по идентификатору.
        /// </summary>
        /// <param name="bookingId">Идентификатор брони.</param>
        /// <returns>Найденная бронь в виде DTO.</returns>
        Task<BookingDto> GetBookingByIdAsync(Guid bookingId);
    }
}
