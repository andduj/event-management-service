using EventManagement.Models;
using FluentValidation;

namespace EventManagement.Application
{
    public class EventValidator : AbstractValidator<Event>
    {
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
