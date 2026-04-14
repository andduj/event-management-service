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

        /// <summary>
        /// Подтверждает бронь и фиксирует время обработки.
        /// </summary>
        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Отклоняет бронь и фиксирует время обработки.
        /// </summary>
        public void Reject()
        {
            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Создает новую бронь в статусе ожидания обработки.
        /// </summary>
        /// <param name="eventId">Идентификатор события.</param>
        /// <returns>Новая бронь.</returns>
        public static Booking Create(Guid eventId)
        {
            return new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
            };
        }
    }
}