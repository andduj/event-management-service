using EventManagement.Auth.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace EventManagement.Auth.Middleware
{
    /// <summary>
    /// Класс для глобальной обработки исключений.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр middleware глобальной обработки исключений.
        /// </summary>
        /// <param name="next">Следующий компонент в конвейере запросов.</param>
        /// <param name="webHostEnvironment">Среда выполнения приложения.</param>
        /// <param name="logger">Логгер приложения.</param>
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// Обрабатывает HTTP-запрос и перехватывает необработанные исключения.
        /// </summary>
        /// <param name="context">Контекст HTTP-запроса.</param>
        /// <returns>Задача выполнения middleware.</returns>
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(
                exception,
                "Необработанное исключение. Метод={0}, Путь={1}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                return;
            }

            int statusCode = MapStatusCode(exception);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = GetTitleForStatusCode(statusCode),
                Detail = exception.Message,
                Instance = context.Request.Path,
            };

            if (_webHostEnvironment.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            }

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static string GetTitleForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status500InternalServerError => "Internal Server Error",
                _ => "An error occurred",
            };
        }

        private static int MapStatusCode(Exception exception)
        {
            return exception switch
            {
                InvalidCredentialsException => StatusCodes.Status404NotFound,
                LoginAlreadyExistsException => StatusCodes.Status400BadRequest,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError,
            };
        }
    }
}
