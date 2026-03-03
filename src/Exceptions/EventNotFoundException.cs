namespace EventManagement.Exceptions
{
    /// <summary>
    /// Исключение, которое возникает при попытке обращения к несуществующему мероприятию.
    /// </summary>
    public class EventNotFoundException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EventNotFoundException"/>
        /// с сообщением по умолчанию "Событие не найдено".
        /// </summary>
        public EventNotFoundException()
            : base("Событие не найдено")
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
        /// с указанным сообщением об ошибке и ссылкой на внутреннее исключение,
        /// которое является причиной возникновения данного исключения.
        /// </summary>
        /// <param name="message">Сообщение, описывающее ошибку.</param>
        /// <param name="innerException">Исключение, которое является причиной текущего исключения.</param>
        public EventNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
