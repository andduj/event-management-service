using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Exceptions;
using EventManagement.Bookings.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Data.Repositories
{
    /// <summary>
    /// Репозиторий для работы с бронированиями, реализующий хранение данных в оперативной памяти.
    /// </summary>
    public class InMemoryBookingRepository : IBookingRepository
    {
        private static readonly List<Booking> _bookings;

        static InMemoryBookingRepository()
        {
            _bookings = new List<Booking>();
        }

        /// <inheritdoc/>
        public Task<Booking> CreateBookingAsync(Booking booking)
        {
            _bookings.Add(booking);
            return Task.FromResult(booking);
        }

        /// <inheritdoc/>
        public Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = _bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null)
            {
                throw new BookingNotFoundException($"Бронь с id={bookingId} не найдена.");
            }

            return Task.FromResult(booking);
        }

        /// <inheritdoc/>
        public Task<IReadOnlyCollection<Booking>> GetBookingsAsync(BookingStatus bookingStatus)
        {
            IReadOnlyCollection<Booking> bookings = _bookings
                .Where(booking => booking.Status == bookingStatus)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(bookings);
        }

        /// <inheritdoc/>
        public Task UpdateBookingAsync(Booking booking)
        {
            var existingBooking = _bookings.FirstOrDefault(item => item.Id == booking.Id);
            if (existingBooking == null)
            {
                throw new BookingNotFoundException($"Бронь с id={booking.Id} не найдена.");
            }

            existingBooking.EventId = booking.EventId;
            existingBooking.Status = booking.Status;
            existingBooking.CreatedAt = booking.CreatedAt;
            existingBooking.ProcessedAt = booking.ProcessedAt;

            return Task.CompletedTask;
        }
    }
}
