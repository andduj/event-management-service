using EventManagement.Events.Application.Requests;
using FluentValidation;

namespace EventManagement.Events.Application.Validators
{
    /// <summary>
    /// Валидатор запроса на обновление мероприятия.
    /// </summary>
    public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
    {
        /// <summary>
        /// Инициализирует правила валидации.
        /// </summary>
        public UpdateEventRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Заголовок мероприятия обязателен")
                .MaximumLength(200).WithMessage("Заголовок не должен превышать 200 символов");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");

            RuleFor(x => x.StartAt)
                .NotEmpty().WithMessage("Дата начала обязательна");

            RuleFor(x => x.EndAt)
                .NotEmpty().WithMessage("Дата окончания обязательна")
                .GreaterThan(x => x.StartAt).WithMessage("Дата окончания должна быть позже даты начала");

            RuleFor(x => x.AvailableSeats)
                .GreaterThanOrEqualTo(0).WithMessage("Количество свободных мест не может быть отрицательным");
        }
    }
}
