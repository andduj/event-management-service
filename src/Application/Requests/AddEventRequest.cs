using System.ComponentModel.DataAnnotations;

namespace EventManagement.Application.Requests
{
    /// <summary>
    /// Запрос на создание нового мероприятия
    /// </summary>
    public class AddEventRequest
    {
        /// <summary>
        /// Заголовок мероприятия
        /// </summary>
        [Required(ErrorMessage = "Заголовок мероприятия обязателен")]
        public string Title { get; set; }

        /// <summary>
        /// Подробное описание мероприятия
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Дата и время начала мероприятия
        /// </summary>
        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата и время окончания мероприятия
        /// </summary>
        [Required(ErrorMessage = "Дата окончания обязательна")]
        [Compare(nameof(StartAt), ErrorMessage = "Дата окончания должна быть позже даты начала")]
        public DateTime EndAt { get; set; }
    }
}