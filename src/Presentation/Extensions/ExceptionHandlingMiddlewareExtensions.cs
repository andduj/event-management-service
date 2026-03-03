using EventManagement.Presentation.Middleware;

namespace EventManagement.Presentation.Extensions
{
    /// <summary>
    /// Класс-расширение для регистрации middleware обработки исключений в конвейере HTTP-запросов.
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Добавляет middleware для глобальной обработки исключений в конвейер обработки запросов.
        /// </summary>
        /// <param name="app">Построитель конвейера обработки запросов приложения.</param>
        /// <returns>Построитель конвейера для дальнейшей настройки.</returns>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
