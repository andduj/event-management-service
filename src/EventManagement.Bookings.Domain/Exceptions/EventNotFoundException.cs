using System;

namespace EventManagement.Bookings.Domain.Exceptions
{
    /// <summary>
    /// Исключение, которое возникает при обращении к несуществующему мероприятию.
    /// </summary>
    public class EventNotFoundException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventNotFoundException"/>
        /// с сообщением по умолчанию.
        /// </summary>
        public EventNotFoundException()
            : base("Мероприятие не найдено")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventNotFoundException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public EventNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventNotFoundException"/>
        /// с указанным сообщением и внутренним исключением.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        /// <param name="innerException">Исключение, которое является причиной текущего исключения.</param>
        public EventNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
