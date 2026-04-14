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
            await _processingSemaphore.WaitAsync(cancellationToken);
            EventDto? eventDto = null;
            try
            {
                eventDto = await _eventsClient.EventsGetAsync(booking.EventId);
                await Task.Delay(DelayInMilliseconds, cancellationToken);
                booking.Confirm();
                await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);
            }
            catch (ApiException exception) when (exception.StatusCode == StatusCodes.Status404NotFound)
            {
                booking.Reject();
                _logger.Warn("Мероприятия с id={0} не существует", booking.EventId);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                booking.Reject();
                await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);
                if (eventDto != null)
                {
                    await _eventsClient.ReleaseSeatsAsync(eventDto.Id, 1, cancellationToken);
                }               

                _logger.Error(exception, "Ошибка при обработке бронирования {0}", booking.Id);
                throw;
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
    }
}
