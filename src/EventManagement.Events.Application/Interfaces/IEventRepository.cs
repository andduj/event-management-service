using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Domain.Exceptions;
using EventManagement.Events.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Application.Interfaces
{
    /// <summary>
    /// Репозиторий для управления мероприятиями.
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Возвращает мероприятия из репозитория с учетом фильтрации и пагинации.
        /// </summary>
        /// <param name="eventFilter">Фильтр для мероприятий.</param>
        /// <param name="page">Номер страницы.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <returns>Результат с данными текущей страницы и метаданными пагинации.</returns>
        Task<PaginatedResult<Event>> FilterAsync(EventFilter eventFilter, int page, int pageSize);

        /// <summary>
        /// Получает конкретное мероприятие по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия (GUID).</param>
        /// <returns>Мероприятие, если найдено.</returns>
        /// <exception cref="EventNotFoundException">
        /// Генерируется, если мероприятие с указанным идентификатором не найдено.
        /// </exception>
        Task<Event> GetEventByIdAsync(Guid id);

        /// <summary>
        /// Возвращает топ мероприятий с наибольшим процентом проданных мест.
        /// </summary>
        /// <param name="count">Максимальное число мероприятий в выборке.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Список мероприятий, отсортированный по убыванию процента продаж.</returns>
        Task<IReadOnlyList<Event>> GetTopPopularAsync(int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавляет новое мероприятие в репозиторий.
        /// </summary>
        /// <param name="newEvent">Объект мероприятия для добавления.</param>
        /// <returns>Добавленное мероприятие с сгенерированными данными (например, ID).</returns>
        Task<Event> CreateEventAsync(Event newEvent);

        /// <summary>
        /// Обновляет существующее мероприятие в репозитории.
        /// </summary>
        /// <param name="updatedEvent">Объект мероприятия с обновленными данными.</param>
        Task UpdateEventAsync(Event updatedEvent);

        /// <summary>
        /// Удаляет мероприятие из репозитория по его идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия для удаления (GUID).</param>
        Task DeleteEventAsync(Guid id);

        /// <summary>
        /// Проверяет существование мероприятия по идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия (GUID).</param>
        /// <returns><c>true</c>, если мероприятие найдено; иначе <c>false</c>.</returns>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task<bool> Exists(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Пытается зарезервировать указанное количество мест для мероприятия.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест для резервирования.</param>
        /// <returns><c>true</c>, если резервирование выполнено; иначе <c>false</c>.</returns>
        Task<bool> TryReserveSeats(Guid id, int count);

        /// <summary>
        /// Освобождает указанное количество мест для мероприятия.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест для освобождения.</param>
        Task ReleaseSeats(Guid id, int count);
    }
}
