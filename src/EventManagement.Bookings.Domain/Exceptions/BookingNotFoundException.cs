using System;

namespace EventManagement.Bookings.Domain.Exceptions
{
    /// <summary>
    /// Исключение, которое возникает при попытке обращения к несуществующей брони.
    /// </summary>
    public class BookingNotFoundException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BookingNotFoundException"/>
        /// с сообщением по умолчанию "Бронь не найдена".
        /// </summary>
        public BookingNotFoundException()
            : base("Бронь не найдена")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BookingNotFoundException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public BookingNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BookingNotFoundException"/>
        /// с указанным сообщением об ошибке и ссылкой на внутреннее исключение,
        /// которое является причиной возникновения данного исключения.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        /// <param name="innerException">Исключение, которое является причиной текущего исключения.</param>
        public BookingNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
