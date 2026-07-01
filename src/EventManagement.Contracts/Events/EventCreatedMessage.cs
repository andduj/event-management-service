using System;

namespace EventManagement.Contracts.Events
{
    /// <summary>
    /// Сообщение о создании мероприятия для репликации в Bookings.
    /// </summary>
    public sealed class EventCreatedMessage
    {
        /// <summary>
        /// Идентификатор мероприятия.
        /// </summary>
        public required Guid EventId { get; init; }

        /// <summary>
        /// Заголовок мероприятия.
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Описание мероприятия.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Дата и время начала (UTC).
        /// </summary>
        public required DateTime StartAtUtc { get; init; }

        /// <summary>
        /// Дата и время окончания (UTC).
        /// </summary>
        public required DateTime EndAtUtc { get; init; }

        /// <summary>
        /// Общее количество мест.
        /// </summary>
        public required int TotalSeats { get; init; }

        /// <summary>
        /// Количество свободных мест.
        /// </summary>
        public required int AvailableSeats { get; init; }
    }
}
