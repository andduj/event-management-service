using System;

namespace EventManagement.Contracts.Bookings
{
    /// <summary>
    /// Сообщение об отменённой брони для освобождения мест в Events.
    /// </summary>
    public sealed class BookingCancelledMessage
    {
        /// <summary>
        /// Идентификатор брони.
        /// </summary>
        public required Guid BookingId { get; init; }

        /// <summary>
        /// Идентификатор мероприятия.
        /// </summary>
        public required Guid EventId { get; init; }

        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public required Guid UserId { get; init; }

        /// <summary>
        /// Количество освобождаемых мест.
        /// </summary>
        public required int SeatsCount { get; init; }
    }
}
