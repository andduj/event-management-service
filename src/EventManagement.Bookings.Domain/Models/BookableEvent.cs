using System;

namespace EventManagement.Bookings.Domain.Models
{
    /// <summary>
    /// Локальная проекция мероприятия для принятия решений о бронировании.
    /// </summary>
    public class BookableEvent
    {
        private BookableEvent()
        {
            Title = null!;
        }

        /// <summary>
        /// Идентификатор мероприятия.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Заголовок мероприятия.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Описание мероприятия.
        /// </summary>
        public string? Description { get; private set; }

        /// <summary>
        /// Дата и время начала мероприятия (UTC).
        /// </summary>
        public DateTime StartAt { get; private set; }

        /// <summary>
        /// Дата и время окончания мероприятия (UTC).
        /// </summary>
        public DateTime EndAt { get; private set; }

        /// <summary>
        /// Общее количество мест.
        /// </summary>
        public int TotalSeats { get; private set; }

        /// <summary>
        /// Количество свободных мест.
        /// </summary>
        public int AvailableSeats { get; private set; }

        /// <summary>
        /// Создаёт проекцию мероприятия из данных синхронизации.
        /// </summary>
        public static BookableEvent Create(
            Guid id,
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats,
            int availableSeats)
        {
            return new BookableEvent
            {
                Id = id,
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt,
                TotalSeats = totalSeats,
                AvailableSeats = availableSeats
            };
        }

        /// <summary>
        /// Обновляет поля проекции из данных синхронизации.
        /// </summary>
        public void Sync(
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats,
            int availableSeats)
        {
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
        }

        /// <summary>
        /// Проверяет, началось ли мероприятие.
        /// </summary>
        public bool HasStarted(DateTime utcNow)
        {
            return StartAt <= utcNow;
        }

        /// <summary>
        /// Пытается зарезервировать указанное число мест.
        /// </summary>
        public bool TryReserveSeats(int count = 1)
        {
            if (count <= 0)
            {
                return false;
            }

            if (count > AvailableSeats)
            {
                return false;
            }

            AvailableSeats -= count;
            return true;
        }

        /// <summary>
        /// Освобождает указанное число мест.
        /// </summary>
        public void ReleaseSeats(int count = 1)
        {
            if (count <= 0)
            {
                return;
            }

            if (count + AvailableSeats > TotalSeats)
            {
                return;
            }

            AvailableSeats += count;
        }
    }
}
