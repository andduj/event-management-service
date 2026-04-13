using System;

namespace EventManagement.Events.Application.Requests
{
    /// <summary>
    /// Запрос на обновление мероприятия.
    /// </summary>
    public class UpdateEventRequest
    {
        /// <summary>
        /// Заголовок мероприятия.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Описание мероприятия.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата и время начала мероприятия.
        /// </summary>
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата и время окончания мероприятия.
        /// </summary>
        public DateTime EndAt { get; set; }

        /// <summary>
        /// Текущее количество свободных мест.
        /// </summary>
        public int AvailableSeats { get; set; }
    }
}