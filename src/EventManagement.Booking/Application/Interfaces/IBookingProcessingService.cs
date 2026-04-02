using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Интерфейс обработки ожидающих бронирований.
    /// </summary>
    public interface IBookingProcessingService
    {
        /// <summary>
        /// Обрабатывает бронирования в статусе Pending.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task ProcessPendingBookingsAsync(CancellationToken cancellationToken);
    }
}
