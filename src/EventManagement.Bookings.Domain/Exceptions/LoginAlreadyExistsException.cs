using System;

namespace EventManagement.Bookings.Domain.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при попытке зарегистрировать уже занятый логин.
    /// </summary>
    public class LoginAlreadyExistsException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LoginAlreadyExistsException"/>
        /// с сообщением по умолчанию «Пользователь с таким логином уже существует».
        /// </summary>
        public LoginAlreadyExistsException()
            : base("Пользователь с таким логином уже существует")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LoginAlreadyExistsException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public LoginAlreadyExistsException(string message)
            : base(message)
        {
        }
    }
}
