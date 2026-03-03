using System.ComponentModel.DataAnnotations;

namespace EventManagement.Application.Requests
{
    /// <summary>
    /// Запрос на создание нового мероприятия
    /// </summary>
    public class AddEventRequest : IValidatableObject
    {
        /// <summary>
        /// Заголовок мероприятия
        /// </summary>
        [Required(ErrorMessage = "Заголовок мероприятия обязателен")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Подробное описание мероприятия
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата и время начала мероприятия
        /// </summary>
        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата и время окончания мероприятия
        /// </summary>
        [Required(ErrorMessage = "Дата окончания обязательна")]
        public DateTime EndAt { get; set; }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult("Дата окончания должна быть позже даты начала", [nameof(EndAt)]);
            }
        }
    }
}