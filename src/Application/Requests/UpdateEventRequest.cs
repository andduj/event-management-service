namespace EventManagement.Application.Requests
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
        /// Дата начала.
        /// </summary>
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата завершения.
        /// </summary>
        public DateTime EndAt { get; set; }
    }
}