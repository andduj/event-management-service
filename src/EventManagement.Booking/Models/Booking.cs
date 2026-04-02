using System;

namespace EventManagement.Bookings.Models
{
    /// <summary>
    /// Бронирование мероприятия.
    /// </summary>
    public class Booking
    {
        /// <summary>
        /// Уникальный идентификатор брони.
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Идентификатор события, к которому относится бронь.
        /// </summary>
        public required Guid EventId { get; set; }

        /// <summary>
        /// Текущий статус брони.
        /// </summary>
        public required BookingStatus Status { get; set; }

        /// <summary>
        /// Дата и время создания брони.
        /// </summary>
        public required DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время обработки брони.
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
    }
}