using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
using EventManagement.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
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
        private readonly IEventsClient _eventsClient;
        private readonly ILogger<BookingProcessingService> _logger;

        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

        /// <summary>
        /// Инициализирует новый экземпляр сервиса фоновой обработки бронирований.
        /// </summary>
        /// <param name="bookingRepository">Репозиторий бронирований.</param>
        /// <param name="eventsClient">Сервис мероприятий.</param>
        /// <param name="logger">Логгер приложения.</param>
        public BookingProcessingService(IBookingRepository bookingRepository, IEventsClient eventsClient, ILogger<BookingProcessingService> logger)
        {
            _bookingRepository = bookingRepository;
            _eventsClient = eventsClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            var pendingBookings = await _bookingRepository.GetBookingsAsync(BookingStatus.Pending);
            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessBookingAsync(Booking booking, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            await Task.Delay(DelayInMilliseconds, cancellationToken);
            await _processingSemaphore.WaitAsync(cancellationToken);
            try
            {
                bool exists = await _eventsClient.ExistsAsync(booking.EventId, cancellationToken);
                if (exists)
                {
                    booking.Confirm();
                }
                else
                {
                    booking.Reject();
                    _logger.Warn("Мероприятия с id={0} не существует", booking.EventId);
                }
                await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await TryReleaseSeats(booking, cancellationToken);

                _logger.Error(exception, "Ошибка при обработке бронирования {0}", booking.Id);
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }

        private async Task TryReleaseSeats(Booking booking, CancellationToken cancellationToken)
        {
            try
            {
                booking.Reject();
                await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);
                await _eventsClient.ReleaseSeatsAsync(booking.EventId, 1, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Не удалось освободить место для EventId={0}", booking.EventId);
            }            
        }
    }
}
