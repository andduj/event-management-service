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
        /// <returns>Информация о созданной брони.</returns>
        Task<BookingInfo> CreateBookingAsync(Guid eventId);

        /// <summary>
        /// Создает бронь для указанного мероприятия от имени пользователя.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Информация о созданной брони.</returns>
        Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId);

        /// <summary>
        /// Получает бронь по идентификатору.
        /// </summary>
        /// <param name="bookingId">Идентификатор брони.</param>
        /// <returns>Найденная бронь в виде DTO.</returns>
        Task<BookingDto> GetBookingByIdAsync(Guid bookingId);
    }
}
