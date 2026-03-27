using AutoMapper;
using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
using GpnDs.UBER.NTC.Calculations.Api;
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
        private readonly IEventsClient _eventsClient;
        private readonly IMapper _mapper;
        private readonly Logging.ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IEventsClient eventsClient,
            IMapper mapper,
            Logging.ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _eventsClient = eventsClient;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            _logger.Info("Создание новой брони. EventId={0}", eventId);
            await _eventsClient.EventsGetAsync(eventId);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
            };

            var addedBooking = await _bookingRepository.CreateBookingAsync(booking);
            _logger.Info("Бронь успешно создана. BookingId={0}", addedBooking.Id);
            return addedBooking;
        }

        /// <inheritdoc/>
        public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
        {
            _logger.Debug("Получение брони по Id={0}", bookingId);
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return _mapper.Map<BookingDto>(booking);
        }
    }
}
