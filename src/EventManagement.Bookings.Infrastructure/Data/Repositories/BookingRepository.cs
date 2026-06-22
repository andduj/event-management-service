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

        /// <inheritdoc />
        public async Task<bool> TryUpdateBookingAsync(
            Booking booking,
            BookingStatus expectedStatus,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int rowsAffected = await _context.Bookings
                .Where(storedBooking => storedBooking.Id == booking.Id && storedBooking.Status == expectedStatus)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(storedBooking => storedBooking.Status, booking.Status)
                        .SetProperty(storedBooking => storedBooking.ProcessedAt, booking.ProcessedAt),
                    cancellationToken);

            return rowsAffected > 0;
        }

        /// <inheritdoc />
        public async Task<int> CountActiveBookingsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .CountAsync(
                    booking => booking.UserId == userId
                        && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
                    cancellationToken);
        }
    }
}
