using EventManagement.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Presentation.Middleware
{
    /// <summary>
    /// Класс для глобальной обработки исключений.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExceptionHandlingMiddleware"/>.
        /// </summary>
        /// <param name="next">Делегат следующего компонента в конвейере обработки запроса.</param>
        /// <param name="env">Информация о среде выполнения приложения (Development/Production).</param>
        /// <param name="logger">Логгер.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Выполняет обработку HTTP-запроса с перехватом возможных исключений.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        /// <summary>
        /// Обрабатывает исключение и формирует структурированный ответ клиенту.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        /// <param name="exception">Перехваченное исключение.</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(
                exception,
                "Необработанное исключение. Метод:{Method}, Путь:{Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                return;
            }

            int statusCode = MapStatusCode(exception);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitleForStatusCode(statusCode),
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            }

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        /// <summary>
        /// Возвращает стандартный заголовок для HTTP-статуса.
        /// </summary>
        /// <param name="statusCode">Код HTTP-статуса.</param>
        /// <returns>Текстовое описание статуса.</returns>
        private static string GetTitleForStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case 400:
                    return "Bad Request";
                case 401:
                    return "Unauthorized";
                case 403:
                    return "Forbidden";
                case 404:
                    return "Not Found";
                case 405:
                    return "Method Not Allowed";
                case 409:
                    return "Conflict";
                case 415:
                    return "Unsupported Media Type";
                case 422:
                    return "Unprocessable Entity";
                case 500:
                    return "Internal Server Error";
                default:
                    return "An error occurred";
            }
        }

        private static int MapStatusCode(Exception exception)
        {
            int statusCode;
            switch (exception)
            {
                case EventNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case NullReferenceException:
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }
            return statusCode;
        }
    }
}
