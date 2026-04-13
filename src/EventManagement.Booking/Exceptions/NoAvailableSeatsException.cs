using System;

namespace EventManagement.Bookings.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при попытке забронировать места, когда свободных мест на мероприятии недостаточно.
    /// </summary>
    public class NoAvailableSeatsException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="NoAvailableSeatsException"/>
        /// с сообщением по умолчанию «Нет свободных мест для этого события».
        /// </summary>
        public NoAvailableSeatsException()
            : base("Нет свободных мест для этого события")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="NoAvailableSeatsException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public NoAvailableSeatsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="NoAvailableSeatsException"/>
        /// с указанным сообщением об ошибке и ссылкой на внутреннее исключение,
        /// которое является причиной возникновения данного исключения.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        /// <param name="innerException">Исключение, которое является причиной текущего исключения.</param>
        public NoAvailableSeatsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
