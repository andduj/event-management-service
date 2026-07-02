using AutoMapper;
using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Logging;
using System;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Services
{
    /// <summary>
    /// Сервис для работы с бронированиями.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IMapper mapper,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId)
        {
            _logger.Info("Создание новой брони. EventId={0}, UserId={1}", eventId, userId);

            int activeBookings = await _bookingRepository.CountActiveBookingsAsync(userId);
            if (activeBookings >= BookingLimits.MaxActiveBookings)
            {
                throw new ActiveBookingsLimitExceededException(BookingLimits.MaxActiveBookings);
            }

            var booking = Booking.Create(eventId, userId);
            var addedBooking = await _bookingRepository.CreateBookingAsync(booking);

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

            booking.Cancel();
            await _bookingRepository.UpdateBookingAsync(booking);
        }
    }
}
