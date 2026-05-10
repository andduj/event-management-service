using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
using EventManagement.Logging;
using System;
using System.Collections.Generic;
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
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventsClient _eventsClient;
        private readonly ILogger<BookingProcessingService> _logger;

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
        public async Task<List<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pendingBookings = await _bookingRepository.GetBookingsAsync(BookingStatus.Pending);
            return pendingBookings.Select(booking => booking.Id).ToList();
        }

        /// <inheritdoc/>
        public async Task ProcessBookingAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking.Status != BookingStatus.Pending)
            {
                return;
            }

            await ProcessBookingCoreAsync(booking, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task ProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            var pendingBookings = await _bookingRepository.GetBookingsAsync(BookingStatus.Pending);
            var tasks = pendingBookings.Select(booking => ProcessBookingCoreAsync(booking, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessBookingCoreAsync(Booking booking, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            _logger.Debug("Начало обработки бронирования. BookingId={0}, EventId={1}", booking.Id, booking.EventId);
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
                _logger.Debug("Завершена обработка бронирования. BookingId={0}, Status={1}", booking.Id, booking.Status);
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
