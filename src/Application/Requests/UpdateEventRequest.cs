using System.ComponentModel.DataAnnotations;

namespace EventManagement.Application.Requests
{
    /// <summary>
    /// Запрос на обновление мероприятия
    /// </summary>
    public class UpdateEventRequest
    {
        /// <summary>
        /// Идентификатор мероприятия
        /// </summary>
        [Required(ErrorMessage = "Идентификатор мероприятия обязателен")]
        public Guid Id { get; set; }

        /// <summary>
        /// Заголовок мероприятия
        /// </summary>
        [Required(ErrorMessage = "Заголовок мероприятия обязателен")]
        public string Title { get; set; }

        /// <summary>
        /// Описание мероприятия
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Дата начала
        /// </summary>
        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата завершения
        /// </summary>
        [Required(ErrorMessage = "Дата завершения обязательна")]
        [Compare(nameof(StartAt), ErrorMessage = "Дата завершения должна быть позже даты начала")]
        public DateTime EndAt { get; set; }
    }
}