using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Application.Requests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с мероприятиями.
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Возвращает мероприятия с учетом фильтрации и пагинации.
        /// </summary>
        /// <param name="eventFilter">Фильтр для мероприятий.</param>
        /// <param name="page">Номер страницы.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <returns>Результат с данными текущей страницы и метаданными пагинации.</returns>
        Task<PaginatedResult<EventDto>> FilterAsync(EventFilter eventFilter, int page, int pageSize);

        /// <summary>
        /// Получает мероприятие по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия (GUID).</param>
        /// <returns>DTO-объект мероприятия с указанным идентификатором.</returns>
        Task<EventDto> GetEventByIdAsync(Guid id);

        /// <summary>
        /// Возвращает топ-10 самых популярных мероприятий по проценту проданных мест.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Список до 10 мероприятий, отсортированный по убыванию популярности.</returns>
        Task<IReadOnlyList<EventDto>> GetTopPopularEventsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Добавляет новое мероприятие в систему.
        /// </summary>
        /// <param name="addEventRequest">Запрос на добавление мероприятия с необходимыми данными.</param>
        /// <returns>DTO-объект созданного мероприятия.</returns>
        Task<EventDto> CreateEventAsync(AddEventRequest addEventRequest);

        /// <summary>
        /// Обновляет информацию о существующем мероприятии.
        /// </summary>
        /// <param name="id">Уникальный идентификатор обновляемого мероприятия.</param>
        /// <param name="updateEventRequest">Запрос на обновление мероприятия с новыми данными.</param>
        Task UpdateEventAsync(Guid id, UpdateEventRequest updateEventRequest);

        /// <summary>
        /// Удаляет мероприятие из системы.
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого мероприятия.</param>
        Task DeleteEventAsync(Guid id);

        /// <summary>
        /// Пытается зарезервировать указанное количество мест для мероприятия.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест для резервирования.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><c>true</c>, если резервирование выполнено; иначе <c>false</c>.</returns>
        Task<bool> TryReserveSeats(Guid id, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Освобождает указанное количество мест для мероприятия.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест для освобождения.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task ReleaseSeats(Guid id, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет существование мероприятия по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <returns><c>true</c>, если мероприятие существует; иначе <c>false</c>.</returns>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task<bool> Exists(Guid id, CancellationToken cancellationToken = default);
    }
}
