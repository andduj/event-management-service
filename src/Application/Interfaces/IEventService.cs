using EventManagement.Application.DTOs;
using EventManagement.Application.Filters;
using EventManagement.Application.Requests;

namespace EventManagement.Application.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с мероприятиями
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Получает список всех мероприятий
        /// </summary>
        /// <param name="eventFilter">Фильтр для мероприятий.</param>
        /// <param name="page">Страница.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <returns>Список всех мероприятий в виде DTO объектов</returns>
        PaginatedResult<EventDto> Filter(EventFilter eventFilter, int page, int pageSize);

        /// <summary>
        /// Получает мероприятие по его уникальному идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия (GUID)</param>
        /// <returns>DTO объект мероприятия с указанным идентификатором</returns>
        EventDto GetById(Guid id);

        /// <summary>
        /// Добавляет новое мероприятие в систему
        /// </summary>
        /// <param name="addEventRequest">Запрос на добавление мероприятия с необходимыми данными</param>
        /// <returns>DTO объект созданного мероприятия</returns>
        EventDto Add(AddEventRequest addEventRequest);

        /// <summary>
        /// Обновляет информацию о существующем мероприятии
        /// </summary>
        /// <param name="id">Уникальный идентификатор обновляемого мероприятия</param>
        /// <param name="updateEventRequest">Запрос на обновление мероприятия с новыми данными</param>
        void Update(Guid id, UpdateEventRequest updateEventRequest);

        /// <summary>
        /// Удаляет мероприятие из системы
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого мероприятия</param>
        void Delete(Guid id);
    }
}
