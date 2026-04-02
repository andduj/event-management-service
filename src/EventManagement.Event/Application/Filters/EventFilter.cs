using System;

namespace EventManagement.Events.Application.Filters
{
    /// <summary>
    /// Параметры фильтрации мероприятий.
    /// </summary>
    public class EventFilter
    {
        /// <summary>
        /// Заголовок мероприятия.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Минимальная дата начала мероприятия (включительно).
        /// </summary>
        public DateTime? StartAt { get; set; }

        /// <summary>
        /// Максимальная дата окончания мероприятия (включительно).
        /// </summary>
        public DateTime? EndAt { get; set; }
    }
}
