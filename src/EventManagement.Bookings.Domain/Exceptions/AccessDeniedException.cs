using System;

namespace EventManagement.Bookings.Domain.Exceptions
{
    /// <summary>
    /// Исключение, возникающее при попытке выполнить операцию без достаточных прав.
    /// </summary>
    public class AccessDeniedException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AccessDeniedException"/>
        /// с сообщением по умолчанию «Недостаточно прав для выполнения операции».
        /// </summary>
        public AccessDeniedException()
            : base("Недостаточно прав для выполнения операции")
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AccessDeniedException"/>
        /// с указанным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        public AccessDeniedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AccessDeniedException"/>
        /// с указанным сообщением об ошибке и ссылкой на внутреннее исключение,
        /// которое является причиной возникновения данного исключения.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        /// <param name="innerException">Исключение, которое является причиной текущего исключения.</param>
        public AccessDeniedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
