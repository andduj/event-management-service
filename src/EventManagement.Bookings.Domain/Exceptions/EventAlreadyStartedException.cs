using System;

namespace EventManagement.Bookings.Domain.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при попытке забронировать мероприятие, которое уже началось.
    /// </summary>
    public class EventAlreadyStartedException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventAlreadyStartedException"/>
        /// с сообщением по умолчанию «Нельзя забронировать мероприятие, которое уже началось».
        /// </summary>
        public EventAlreadyStartedException()
            : base("Нельзя забронировать мероприятие, которое уже началось")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventAlreadyStartedException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public EventAlreadyStartedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventAlreadyStartedException"/>
        /// с указанным сообщением об ошибке и ссылкой на внутреннее исключение,
        /// которое является причиной возникновения данного исключения.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        /// <param name="innerException">Исключение, которое является причиной текущего исключения.</param>
        public EventAlreadyStartedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
