using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Infrastructure.DataAccess;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Bookings.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Репозиторий бронирований на базе EF Core.
    /// </summary>
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingsDbContext _context;

        /// <summary>
        /// Инициализирует репозиторий бронирований.
        /// </summary>
        /// <param name="context">Контекст данных бронирований.</param>
        public BookingRepository(BookingsDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        /// <inheritdoc />
        public async Task<Booking> GetBookingByIdAsync(Guid bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
            {
                throw new BookingNotFoundException($"Бронь с id={bookingId} не найдена.");
            }

            return booking;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<Booking>> GetBookingsAsync(BookingStatus bookingStatus)
        {
            var bookings = await _context.Bookings
                .Where(booking => booking.Status == bookingStatus)
                .ToListAsync();

            return bookings.AsReadOnly();
        }

        /// <inheritdoc />
        public async Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
