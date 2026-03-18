using FluentValidation;

namespace EventManagement.Application.Extensions
{
    public static class ValidatorExtensions
    {
        public static void ValidateAndThrow<T>(this IValidator<T> validator, T instance)
        {
            var result = validator.Validate(instance);
            if (result.IsValid)
            {
                return;
            }

            throw new ValidationException(result.Errors);
        }
    }
}
