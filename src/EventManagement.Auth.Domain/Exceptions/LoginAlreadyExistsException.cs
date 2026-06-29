using System;

namespace EventManagement.Auth.Domain.Exceptions
{
    /// <summary>
    /// Исключение при попытке зарегистрировать уже занятый логин.
    /// </summary>
    public class LoginAlreadyExistsException : Exception
    {
        public LoginAlreadyExistsException()
            : base("Пользователь с таким логином уже существует")
        {
        }

        public LoginAlreadyExistsException(string message)
            : base(message)
        {
        }
    }
}
