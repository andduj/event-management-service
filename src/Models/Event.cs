namespace EventManagement.Models
{
    /// <summary>
    /// Мероприятие
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Идентификатор мероприятия
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Заголовок мероприятия
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Описание мероприятия
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата завершения
        /// </summary>
        public DateTime EndAt { get; set; }
    }
}
