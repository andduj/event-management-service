using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Интерфейс обработки ожидающих бронирований.
    /// </summary>
    public interface IBookingProcessingService
    {
        /// <summary>
        /// Возвращает идентификаторы бронирований в статусе Pending.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task<List<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Обрабатывает одно бронирование.
        /// </summary>
        /// <param name="bookingId">Идентификатор бронирования.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken);

        /// <summary>
        /// Обрабатывает бронирования в статусе Pending.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task ProcessPendingBookingsAsync(CancellationToken cancellationToken);
    }
}
