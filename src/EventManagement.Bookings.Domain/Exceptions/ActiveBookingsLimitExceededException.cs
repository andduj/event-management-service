using System;

namespace EventManagement.Bookings.Domain.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при превышении лимита активных бронирований пользователя.
    /// </summary>
    public class ActiveBookingsLimitExceededException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ActiveBookingsLimitExceededException"/>
        /// с сообщением о превышении указанного лимита.
        /// </summary>
        /// <param name="limit">Максимальное количество активных бронирований.</param>
        public ActiveBookingsLimitExceededException(int limit)
            : base($"Превышен лимит активных бронирований ({limit})")
        {
            Limit = limit;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ActiveBookingsLimitExceededException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public ActiveBookingsLimitExceededException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ActiveBookingsLimitExceededException"/>
        /// с указанным сообщением об ошибке и ссылкой на внутреннее исключение,
        /// которое является причиной возникновения данного исключения.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        /// <param name="innerException">Исключение, которое является причиной текущего исключения.</param>
        public ActiveBookingsLimitExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Лимит активных бронирований, который был превышен.
        /// </summary>
        public int? Limit { get; }
    }
}
