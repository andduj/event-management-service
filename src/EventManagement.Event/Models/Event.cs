using System;
using System.Threading;
using FluentValidation;

namespace EventManagement.Events.Models
{
    /// <summary>
    /// Мероприятие.
    /// </summary>
    public class Event
    {
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
        /// <returns></returns>
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
            if (totalSeats <= 0)
            {
                throw new ValidationException("Значение должно быть больше 0");
            }

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
