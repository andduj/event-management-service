using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Services
{
    /// <summary>
    /// Сервис фоновой обработки бронирований.
    /// </summary>
    public class BookingProcessingService : IBookingProcessingService
    {
        private const int DelayInMilliseconds = 2000;
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingProcessingService> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса фоновой обработки бронирований.
        /// </summary>
        /// <param name="bookingRepository">Репозиторий бронирований.</param>
        /// <param name="logger">Логгер приложения.</param>
        public BookingProcessingService(IBookingRepository bookingRepository, ILogger<BookingProcessingService> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetBookingsAsync(BookingStatus.Pending);

            foreach (var booking in bookings)
            {
                try
                {
                    await Task.Delay(DelayInMilliseconds, cancellationToken);
                    booking.Status = BookingStatus.Confirmed;
                    booking.ProcessedAt = DateTime.UtcNow;
                    await _bookingRepository.UpdateBookingAsync(booking);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Ошибка при обработке бронирования {0}", booking.Id);
                }
            }
        }
    }
}
