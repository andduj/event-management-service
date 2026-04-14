using AutoMapper;
using EventManagement.Bookings.Application.DTOs;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Exceptions;
using EventManagement.Bookings.Models;
using EventManagement.Events.Api;
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
        private readonly IEventsClient _eventsClient;
        private readonly IMapper _mapper;
        private readonly Logging.ILogger<BookingService> _logger;

        private readonly SemaphoreSlim _semaphoreSlim = new(1,1);

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
        public async Task<BookingInfo> CreateBookingAsync(Guid eventId)
        {
            _logger.Info("Создание новой брони. EventId={0}", eventId);

            Booking addedBooking;
            await _semaphoreSlim.WaitAsync();
            try
            {
                bool wasReserved = await _eventsClient.ReserveSeatsAsync(eventId, 1);
                if (!wasReserved)
                {
                    throw new NoAvailableSeatsException();
                }
                var booking = Booking.Create(eventId);
                addedBooking = await _bookingRepository.CreateBookingAsync(booking);
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
    }
}
