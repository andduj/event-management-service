using AutoMapper;
using EventManagement.Bookings.Application;
using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Services
{
    /// <summary>
    /// Сервис для работы с бронированиями.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookableEventRepository _bookableEventRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public BookingService(
            IBookingRepository bookingRepository,
            IBookableEventRepository bookableEventRepository,
            IMapper mapper,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _bookableEventRepository = bookableEventRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId)
        {
            _logger.Info("Создание новой брони. EventId={0}, UserId={1}", eventId, userId);

            var bookableEvent = await _bookableEventRepository.TryGetByIdAsync(eventId)
                ?? throw new EventNotFoundException($"Мероприятие с id={eventId} не найдено.");

            if (bookableEvent.HasStarted(DateTime.UtcNow))
            {
                throw new EventAlreadyStartedException();
            }

            int activeBookings = await _bookingRepository.CountActiveBookingsAsync(userId);
            if (activeBookings >= BookingLimits.MaxActiveBookings)
            {
                throw new ActiveBookingsLimitExceededException(BookingLimits.MaxActiveBookings);
            }

            Booking addedBooking;
            bool seatReserved = false;
            await _semaphoreSlim.WaitAsync();
            try
            {
                bool wasReserved = await _bookableEventRepository.TryReserveSeatsAsync(eventId, 1);
                if (!wasReserved)
                {
                    throw new NoAvailableSeatsException();
                }

                seatReserved = true;
                var booking = Booking.Create(eventId, userId);
                addedBooking = await _bookingRepository.CreateBookingAsync(booking);
                seatReserved = false;
            }
            catch
            {
                if (seatReserved)
                {
                    await TryReleaseSeatAsync(eventId);
                }

                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            _logger.Info("Бронь успешно создана. BookingId={0}", addedBooking.Id);
            return _mapper.Map<BookingInfo>(addedBooking);
        }

        /// <inheritdoc/>
        public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
        {
            _logger.Debug("Получение брони по Id={0}", bookingId);
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return _mapper.Map<BookingDto>(booking);
        }

        /// <inheritdoc/>
        public async Task CancelBookingAsync(Guid bookingId, Guid userId, UserRole role)
        {
            _logger.Info("Отмена брони. BookingId={0}, UserId={1}, Role={2}", bookingId, userId, role);

            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking.UserId != userId && role != UserRole.Admin)
            {
                throw new AccessDeniedException();
            }

            bool shouldReleaseSeat = booking.IsActive;
            booking.Cancel();
            await _bookingRepository.UpdateBookingAsync(booking);

            if (shouldReleaseSeat)
            {
                await TryReleaseSeatAsync(booking.EventId);
            }
        }

        private async Task TryReleaseSeatAsync(Guid eventId)
        {
            try
            {
                await _bookableEventRepository.ReleaseSeatsAsync(eventId, 1);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Не удалось освободить место для EventId={0}", eventId);
            }
        }
    }
}
