using FluentValidation;

namespace EventManagement.Application.Extensions
{
    /// <summary>
    /// Вспомогательные расширения для запуска FluentValidation-валидации.
    /// </summary>
    public static class ValidatorExtensions
    {
        /// <summary>
        /// Валидирует объект и выбрасывает <see cref="ValidationException"/>, если валидация не пройдена.
        /// </summary>
        /// <typeparam name="T">Тип валидируемого объекта.</typeparam>
        /// <param name="validator">Валидатор объекта.</param>
        /// <param name="instance">Экземпляр для валидации.</param>
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
