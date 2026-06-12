using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Порт для взаимодействия с Events API.
    /// </summary>
    public interface IEventsGateway
    {
        /// <summary>
        /// Проверяет существование мероприятия.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <exception cref="Exceptions.EventsGatewayException">Мероприятие недоступно или запрос завершился ошибкой.</exception>
        Task EnsureEventExistsAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Пытается зарезервировать указанное число мест на мероприятии.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><c>true</c>, если места успешно зарезервированы; иначе <c>false</c>.</returns>
        Task<bool> ReserveSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет, существует ли мероприятие.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><c>true</c>, если мероприятие существует; иначе <c>false</c>.</returns>
        Task<bool> EventExistsAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Освобождает указанное число мест на мероприятии.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task ReleaseSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default);
    }
}
