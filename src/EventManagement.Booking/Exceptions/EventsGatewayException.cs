using System;

namespace EventManagement.Bookings.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при ошибке обращения к Events API.
    /// </summary>
    public class EventsGatewayException : Exception
    {
        /// <summary>
        /// HTTP-код ответа Events API.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventsGatewayException"/>.
        /// </summary>
        /// <param name="message">Сообщение об ошибке.</param>
        /// <param name="statusCode">HTTP-код ответа Events API.</param>
        /// <param name="innerException">Внутреннее исключение.</param>
        public EventsGatewayException(string message, int statusCode, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
