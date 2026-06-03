using EventManagement.Bookings.Domain.Models;
using System;

namespace EventManagement.Bookings.Application.DTOs
{
    /// <summary>
    /// DTO для передачи данных о бронировании.
    /// </summary>
    public class BookingDto
    {
        /// <summary>
        /// Уникальный идентификатор брони.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор мероприятия, к которому относится бронь.
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Текущий статус брони.
        /// </summary>
        public BookingStatus Status { get; set; }

        /// <summary>
        /// Дата и время создания брони.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Дата и время обработки брони.
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
    }
}
