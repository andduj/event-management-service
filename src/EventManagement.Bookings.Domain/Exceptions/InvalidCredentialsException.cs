using System;

namespace EventManagement.Bookings.Domain.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при неверных учётных данных при входе.
    /// </summary>
    public class InvalidCredentialsException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InvalidCredentialsException"/>
        /// с сообщением по умолчанию «Неверные учётные данные».
        /// </summary>
        public InvalidCredentialsException()
            : base("Неверные учётные данные")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InvalidCredentialsException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public InvalidCredentialsException(string message)
            : base(message)
        {
        }
    }
}
