using System.ComponentModel.DataAnnotations;

namespace EventManagement.Application.Requests
{
    /// <summary>
    /// Запрос на обновление мероприятия
    /// </summary>
    public class UpdateEventRequest : IValidatableObject
    {
        /// <summary>
        /// Заголовок мероприятия
        /// </summary>
        [Required(ErrorMessage = "Заголовок мероприятия обязателен")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Описание мероприятия
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Дата начала
        /// </summary>
        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// Дата завершения
        /// </summary>
        [Required(ErrorMessage = "Дата завершения обязательна")]
        public DateTime EndAt { get; set; }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndAt <= StartAt)
            {
                yield return new ValidationResult(
                    "Дата завершения должна быть позже даты начала",
                    new[] { nameof(EndAt) }
                );
            }
        }
    }
}