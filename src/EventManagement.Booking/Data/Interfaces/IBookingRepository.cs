using EventManagement.Bookings.Models;
using System;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Data.Interfaces
{
    /// <summary>
    /// Репозиторий для управления бронированиями.
    /// </summary>
    public interface IBookingRepository
    {
        /// <summary>
        /// Добавляет новую бронь.
        /// </summary>
        /// <param name="booking">Бронь для добавления.</param>
        /// <returns>Добавленная бронь.</returns>
        Task<Booking> CreateBookingAsync(Booking booking);

        /// <summary>
        /// Получает бронь по идентификатору.
        /// </summary>
        /// <param name="bookingId">Идентификатор брони.</param>
        /// <returns>Найденная бронь.</returns>
        Task<Booking> GetBookingByIdAsync(Guid bookingId);
    }
}
