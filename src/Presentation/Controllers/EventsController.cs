using EventManagement.Application.DTOs;
using EventManagement.Application.Filters;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Presentation.Controllers
{
    /// <summary>
    /// Контролер для работы с мероприятиями
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        /// <summary>
        /// Конструктор контролера мероприятий
        /// </summary>
        /// <param name="eventService">Сервис мероприятий</param>
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Создает новое мероприятие
        /// </summary>
        /// <param name="addEventRequest">Данные для создания мероприятия</param>
        /// <returns>Возвращает созданное мероприятие с кодом 201 (Created)</returns>
        /// <response code="201">Мероприятие успешно создано</response>
        /// <response code="400">Некорректные данные запроса</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Add([FromBody] AddEventRequest addEventRequest)
        {
            var addedEvent = _eventService.Add(addEventRequest);
            return CreatedAtAction(nameof(GetById), new { id = addedEvent.Id }, addedEvent);
        }

        /// <summary>
        /// Удаляет мероприятие по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns>Возвращает код 204 (No Content) при успешном удалении</returns>
        /// <response code="204">Мероприятие успешно удалено</response>
        /// <response code="404">Мероприятие с указанным id не найдено</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(Guid id)
        {
            _eventService.Delete(id);
            return NoContent();
        }

        /// <summary>
        /// Получает список всех мероприятий
        /// </summary>
        /// <param name="eventFilter">Фильтр для мероприятий.</param>
        /// <param name="page">Страница.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <returns>Возвращает список мероприятий с кодом 200 (OK)</returns>
        /// <response code="200">Список мероприятий успешно получен</response>
        [HttpPost("Filter")]
        [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
        public ActionResult<PaginatedResult<EventDto>> Filter([FromBody]EventFilter eventFilter, int? page = 1, int? pageSize = 10)
        {
            var events = _eventService.Filter(eventFilter, page!.Value, pageSize!.Value);
            return Ok(events);
        }

        /// <summary>
        /// Получает мероприятие по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns>Возвращает мероприятие с кодом 200 (OK)</returns>
        /// <response code="200">Мероприятие успешно найдено</response>
        /// <response code="404">Мероприятие с указанным id не найдено</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<EventDto> GetById(Guid id)
        {
            var eventItem = _eventService.GetById(id);
            return Ok(eventItem);
        }

        /// <summary>
        /// Обновляет существующее мероприятие
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <param name="updateEventRequest">Данные для обновления мероприятия</param>
        /// <returns>Возвращает код 204 (No Content) при успешном обновлении</returns>
        /// <response code="204">Мероприятие успешно обновлено</response>
        /// <response code="400">Некорректные данные запроса</response>
        /// <response code="404">Мероприятие с указанным id не найдено</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Update(Guid id, [FromBody] UpdateEventRequest updateEventRequest)
        {
            _eventService.Update(id, updateEventRequest);
            return NoContent();
        }
    }
}
