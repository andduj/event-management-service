using EventManagement.Models;
using FluentValidation;

namespace EventManagement.Application
{
    /// <summary>
    /// Валидатор доменной модели мероприятия.
    /// </summary>
    public class EventValidator : AbstractValidator<Event>
    {
        /// <summary>
        /// Инициализирует новый экземпляр валидатора мероприятия.
        /// </summary>
        public EventValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Заголовок мероприятия обязателен");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание мероприятия обязательно");

            RuleFor(x => x.StartAt)
                .NotEmpty().WithMessage("Дата начала обязательна");

            RuleFor(x => x.EndAt)
                .NotEmpty().WithMessage("Дата окончания обязательна")
                .GreaterThan(x => x.StartAt).WithMessage("Дата окончания должна быть позже даты начала");
        }
    }
}
