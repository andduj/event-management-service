using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Exceptions;
using EventManagement.Events.Models;
using System;
using System.Threading.Tasks;

namespace EventManagement.Events.Data.Interfaces
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
    }
}
