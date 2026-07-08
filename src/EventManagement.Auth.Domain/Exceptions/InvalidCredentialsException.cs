using System;

namespace EventManagement.Auth.Domain.Exceptions
{
    /// <summary>
    /// Исключение при неверных учётных данных при входе.
    /// </summary>
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException()
            : base("Неверные учётные данные")
        {
        }

        public InvalidCredentialsException(string message)
            : base(message)
        {
        }
    }
}
