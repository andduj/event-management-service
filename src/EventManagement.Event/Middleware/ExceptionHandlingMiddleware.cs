using EventManagement.Events.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Events.Middleware
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
        public ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment webHostEnvironment, ILogger<ExceptionHandlingMiddleware> logger)
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

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitleForStatusCode(statusCode),
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            if (exception is ValidationException validationException)
            {
                problemDetails.Title = "Validation Error";

                var errors = validationException.Errors
                    .GroupBy(exception => exception.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(e => e.ErrorMessage).ToArray()
                    );

                problemDetails.Extensions["errors"] = errors;
            }

            if (_webHostEnvironment.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            }

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

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
                case ValidationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case EventNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    break;
                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
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