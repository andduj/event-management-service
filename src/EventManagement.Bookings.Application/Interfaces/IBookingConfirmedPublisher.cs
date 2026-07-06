using EventManagement.Bookings.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Публикация подтверждённой брони в Kafka.
    /// </summary>
    public interface IBookingConfirmedPublisher
    {
        /// <summary>
        /// Публикует сообщение о подтверждённой брони.
        /// </summary>
        /// <param name="booking">Подтверждённая бронь.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task PublishAsync(Booking booking, CancellationToken cancellationToken = default);
    }
}
