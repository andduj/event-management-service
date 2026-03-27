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
    }
}
