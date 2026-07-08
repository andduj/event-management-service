using System;

namespace EventManagement.Contracts.Bookings
{
    /// <summary>
    /// Сообщение о подтверждённой брони для уменьшения мест в Events.
    /// </summary>
    public sealed class BookingConfirmedMessage
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
        /// Количество зарезервированных мест.
        /// </summary>
        public required int SeatsCount { get; init; }
    }
}
