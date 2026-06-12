using System;

namespace EventManagement.Events.Domain.Models
{
    /// <summary>
    /// Мероприятие.
    /// </summary>
    public class Event
    {
        private Event()
        {
            Title = null!;
        }

        /// <summary>
        /// Идентификатор мероприятия.
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Заголовок мероприятия.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Описание мероприятия.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата и время начала мероприятия.
        /// </summary>
        public required DateTime StartAt { get; set; }

        /// <summary>
        /// Дата и время окончания мероприятия.
        /// </summary>
        public required DateTime EndAt { get; set; }

        /// <summary>
        /// Общее количество мест на событии.
        /// </summary>
        public int TotalSeats { get; set; }

        /// <summary>
        /// Текущее количество свободных мест.
        /// </summary>
        public int AvailableSeats { get; set; }

        /// <summary>
        /// Резервирование мест.
        /// </summary>
        /// <param name="count">Число мест.</param>
        /// <returns><c>true</c>, если места зарезервированы; иначе <c>false</c>.</returns>
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
        /// Устанавливает количество свободных мест.
        /// </summary>
        /// <param name="availableSeats">Новое количество свободных мест.</param>
        public void SetAvailableSeats(int availableSeats)
        {
            if (availableSeats > TotalSeats)
            {
                throw new ArgumentOutOfRangeException(nameof(availableSeats), "Количество свободных мест не может превышать общее число мест");
            }

            AvailableSeats = availableSeats;
        }

        /// <summary>
        /// Освобождение мест при отклонении брони.
        /// </summary>
        /// <param name="count">Число мест.</param>
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

        /// <summary>
        /// Создает экземпляр мероприятия с инициализацией количества мест.
        /// </summary>
        /// <param name="title">Заголовок мероприятия.</param>
        /// <param name="startAt">Дата и время начала мероприятия.</param>
        /// <param name="endAt">Дата и время окончания мероприятия.</param>
        /// <param name="totalSeats">Общее количество мест на мероприятии.</param>
        /// <param name="description">Описание мероприятия.</param>
        /// <returns>Новый экземпляр <see cref="Event"/>.</returns>
        public static Event Create(
            string title,
            DateTime startAt,
            DateTime endAt,
            int totalSeats,
            string? description = null)
        {
            return new Event
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt,
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats
            };
        }
    }
}
