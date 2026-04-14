using EventManagement.Bookings.Models;
using System;
using System.Collections.Generic;
using System.Threading;
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

        /// <summary>
        /// Возвращает список бронирований с указанным статусом.
        /// </summary>
        /// <param name="bookingStatus">Статус бронирований для выборки.</param>
        /// <returns>Список бронирований с указанным статусом.</returns>
        Task<IReadOnlyCollection<Booking>> GetBookingsAsync(BookingStatus bookingStatus);

        /// <summary>
        /// Обновляет существующую бронь.
        /// </summary>
        /// <param name="booking">Бронь с обновленными данными.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default);
    }
}
