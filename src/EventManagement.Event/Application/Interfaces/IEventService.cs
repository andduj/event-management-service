using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Application.Requests;
using System;

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
        PaginatedResult<EventDto> Filter(EventFilter eventFilter, int page, int pageSize);

        /// <summary>
        /// Получает мероприятие по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия (GUID).</param>
        /// <returns>DTO-объект мероприятия с указанным идентификатором.</returns>
        EventDto GetById(Guid id);

        /// <summary>
        /// Добавляет новое мероприятие в систему.
        /// </summary>
        /// <param name="addEventRequest">Запрос на добавление мероприятия с необходимыми данными.</param>
        /// <returns>DTO-объект созданного мероприятия.</returns>
        EventDto Add(AddEventRequest addEventRequest);

        /// <summary>
        /// Обновляет информацию о существующем мероприятии.
        /// </summary>
        /// <param name="id">Уникальный идентификатор обновляемого мероприятия.</param>
        /// <param name="updateEventRequest">Запрос на обновление мероприятия с новыми данными.</param>
        void Update(Guid id, UpdateEventRequest updateEventRequest);

        /// <summary>
        /// Удаляет мероприятие из системы.
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого мероприятия.</param>
        void Delete(Guid id);
    }
}
