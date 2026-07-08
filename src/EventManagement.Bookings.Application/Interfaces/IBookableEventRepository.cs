using EventManagement.Bookings.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Репозиторий локальной проекции мероприятий.
    /// </summary>
    public interface IBookableEventRepository
    {
        /// <summary>
        /// Возвращает проекцию мероприятия по идентификатору.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Проекция мероприятия или <c>null</c>, если не найдена.</returns>
        Task<BookableEvent?> TryGetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет существование проекции мероприятия.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><c>true</c>, если проекция существует; иначе <c>false</c>.</returns>
        Task<bool> ExistsAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Создаёт или обновляет проекцию мероприятия.
        /// </summary>
        /// <param name="bookableEvent">Проекция мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task UpsertAsync(BookableEvent bookableEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет проекцию мероприятия.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task DeleteAsync(Guid eventId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Атомарно пытается зарезервировать места.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><c>true</c>, если места зарезервированы; иначе <c>false</c>.</returns>
        Task<bool> TryReserveSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Освобождает зарезервированные места.
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task ReleaseSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default);
    }
}
