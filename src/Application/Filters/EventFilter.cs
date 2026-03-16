namespace EventManagement.Application.Filters
{
    /// <summary>
    /// Фильтр для мероприятияй.
    /// </summary>
    public class EventFilter
    {
        /// <summary>
        /// Заголовок мероприятия.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Дата и время начала мероприятия.
        /// </summary>
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// Дата и время окончания мероприятия.
        /// </summary>
        public DateTime? EndAt { get; set; }
    }
}
