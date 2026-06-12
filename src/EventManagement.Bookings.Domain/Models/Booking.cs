using System;

namespace EventManagement.Bookings.Domain.Models
{
    /// <summary>
    /// Бронирование мероприятия.
    /// </summary>
    public class Booking
    {
        private Booking()
        {
        }

        /// <summary>
        /// Уникальный идентификатор брони.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Идентификатор события, к которому относится бронь.
        /// </summary>
        public Guid EventId { get; private set; }

        /// <summary>
        /// Текущий статус брони.
        /// </summary>
        public BookingStatus Status { get; private set; }

        /// <summary>
        /// Дата и время создания брони.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Дата и время обработки брони.
        /// </summary>
        public DateTime? ProcessedAt { get; private set; }

        /// <summary>
        /// Подтверждает бронь и фиксирует время обработки.
        /// </summary>
        public void Confirm()
        {
            if (Status == BookingStatus.Confirmed)
            {
                return;
            }

            if (Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException($"Нельзя подтвердить бронь в статусе {Status}.");
            }

            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Отклоняет бронь и фиксирует время обработки.
        /// </summary>
        public void Reject()
        {
            if (Status == BookingStatus.Rejected)
            {
                return;
            }

            if (Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException($"Нельзя отклонить бронь в статусе {Status}.");
            }

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
