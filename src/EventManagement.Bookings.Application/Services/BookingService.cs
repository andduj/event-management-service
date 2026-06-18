using AutoMapper;
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
        private readonly IEventsGateway _eventsGateway;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;

        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public BookingService(
            IBookingRepository bookingRepository,
            IEventsGateway eventsGateway,
            IMapper mapper,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _eventsGateway = eventsGateway;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<BookingInfo> CreateBookingAsync(Guid eventId) =>
            throw new NotImplementedException(
                "Вызов без userId устарел. Контроллер будет обновлён на этапе JWT.");

        /// <inheritdoc/>
        public async Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId)
        {
            _logger.Info("Создание новой брони. EventId={0}, UserId={1}", eventId, userId);
            await _eventsGateway.EnsureEventExistsAsync(eventId);

            Booking addedBooking;
            bool seatReserved = false;
            await _semaphoreSlim.WaitAsync();
            try
            {
                bool wasReserved = await _eventsGateway.ReserveSeatsAsync(eventId, 1);
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

        private async Task TryReleaseSeatAsync(Guid eventId)
        {
            try
            {
                await _eventsGateway.ReleaseSeatsAsync(eventId, 1);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Не удалось освободить место для EventId={0} после ошибки создания брони", eventId);
            }
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
