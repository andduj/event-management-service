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
        /// Идентификатор пользователя, создавшего бронь.
        /// </summary>
        public Guid UserId { get; private set; }

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
        /// Признак активной брони (ожидает обработки или подтверждена).
        /// </summary>
        public bool IsActive => Status is BookingStatus.Pending or BookingStatus.Confirmed;

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
        /// Отменяет бронь и фиксирует время обработки.
        /// </summary>
        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
            {
                return;
            }

            if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException($"Нельзя отменить бронь в статусе {Status}.");
            }

            Status = BookingStatus.Cancelled;
            ProcessedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Создает новую бронь в статусе ожидания обработки.
        /// </summary>
        /// <param name="eventId">Идентификатор события.</param>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Новая бронь.</returns>
        public static Booking Create(Guid eventId, Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Идентификатор пользователя не может быть пустым.", nameof(userId));
            }

            return new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
            };
        }
    }
}
