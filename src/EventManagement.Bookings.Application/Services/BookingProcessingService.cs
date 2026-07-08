using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Models;
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
        private readonly IBookableEventRepository _bookableEventRepository;
        private readonly IBookingConfirmedPublisher _bookingConfirmedPublisher;
        private readonly ILogger<BookingProcessingService> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса фоновой обработки бронирований.
        /// </summary>
        /// <param name="bookingRepository">Репозиторий бронирований.</param>
        /// <param name="bookableEventRepository">Репозиторий локальных проекций мероприятий.</param>
        /// <param name="bookingConfirmedPublisher">Издатель подтверждённых броней в Kafka.</param>
        /// <param name="logger">Логгер приложения.</param>
        public BookingProcessingService(
            IBookingRepository bookingRepository,
            IBookableEventRepository bookableEventRepository,
            IBookingConfirmedPublisher bookingConfirmedPublisher,
            ILogger<BookingProcessingService> logger)
        {
            _bookingRepository = bookingRepository;
            _bookableEventRepository = bookableEventRepository;
            _bookingConfirmedPublisher = bookingConfirmedPublisher;
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

        private async Task ProcessBookingCoreAsync(Booking booking, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _logger.Debug("Начало обработки бронирования. BookingId={0}, EventId={1}", booking.Id, booking.EventId);
            try
            {
                bool exists = await _bookableEventRepository.ExistsAsync(booking.EventId, cancellationToken);
                if (exists)
                {
                    booking.Confirm();
                    if (!await TryPersistStatusChangeAsync(booking, cancellationToken))
                    {
                        return;
                    }

                    await PublishBookingConfirmedOrCompensateAsync(booking, cancellationToken);
                }
                else
                {
                    _logger.Warn("Мероприятия с id={0} не существует", booking.EventId);
                    await RejectAndReleaseSeatsAsync(booking, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await RejectAndReleaseSeatsAsync(booking, cancellationToken);
                _logger.Error(exception, "Ошибка при обработке бронирования {0}", booking.Id);
            }
            finally
            {
                _logger.Debug("Завершена обработка бронирования. BookingId={0}, Status={1}", booking.Id, booking.Status);
            }
        }

        private async Task<bool> TryPersistStatusChangeAsync(Booking booking, CancellationToken cancellationToken)
        {
            bool wasUpdated = await _bookingRepository.TryUpdateBookingAsync(booking, BookingStatus.Pending, cancellationToken);
            if (!wasUpdated)
            {
                _logger.Debug("Бронирование {0} уже обработано другим процессом", booking.Id);
            }

            return wasUpdated;
        }

        private async Task PublishBookingConfirmedOrCompensateAsync(Booking booking, CancellationToken cancellationToken)
        {
            try
            {
                await _bookingConfirmedPublisher.PublishConfirmedAsync(booking, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.Error(
                    exception,
                    "Не удалось опубликовать booking-confirmed. BookingId={0}",
                    booking.Id);

                await RejectAndReleaseSeatsAsync(
                    booking,
                    BookingStatus.Confirmed,
                    cancellationToken);
            }
        }

        private async Task RejectAndReleaseSeatsAsync(Booking booking, CancellationToken cancellationToken)
        {
            await RejectAndReleaseSeatsAsync(booking, BookingStatus.Pending, cancellationToken);
        }

        private async Task RejectAndReleaseSeatsAsync(
            Booking booking,
            BookingStatus expectedStatus,
            CancellationToken cancellationToken)
        {
            try
            {
                if (expectedStatus == BookingStatus.Confirmed)
                {
                    booking.Cancel();
                }
                else
                {
                    booking.Reject();
                }

                bool wasUpdated = await _bookingRepository.TryUpdateBookingAsync(booking, expectedStatus, cancellationToken);
                if (!wasUpdated)
                {
                    return;
                }

                await _bookableEventRepository.ReleaseSeatsAsync(booking.EventId, 1, cancellationToken);
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
