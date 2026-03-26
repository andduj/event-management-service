using System;

namespace EventManagement.Events.Application.Requests
{
    /// <summary>
    /// Запрос на создание нового мероприятия.
    /// </summary>
    public class AddEventRequest
    {
        /// <summary>
        /// Заголовок мероприятия.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Подробное описание мероприятия.
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
    }
}