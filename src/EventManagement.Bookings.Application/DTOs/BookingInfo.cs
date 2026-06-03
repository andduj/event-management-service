using EventManagement.Bookings.Domain.Models;
using System;

namespace EventManagement.Bookings.Application.DTOs
{
    /// <summary>
    /// Информацию о созданной брони.
    /// </summary>
    public class BookingInfo
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
    }
}
