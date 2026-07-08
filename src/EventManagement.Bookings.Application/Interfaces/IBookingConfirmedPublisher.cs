using EventManagement.Bookings.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Публикация событий по изменению статуса брони в Kafka.
    /// </summary>
    public interface IBookingConfirmedPublisher
    {
        /// <summary>
        /// Публикует сообщение о подтверждённой брони.
        /// </summary>
        /// <param name="booking">Подтверждённая бронь.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task PublishConfirmedAsync(Booking booking, CancellationToken cancellationToken = default);

        /// <summary>
        /// Публикует сообщение об отменённой брони.
        /// </summary>
        /// <param name="booking">Отменённая бронь.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task PublishCancelledAsync(Booking booking, CancellationToken cancellationToken = default);
    }
}
