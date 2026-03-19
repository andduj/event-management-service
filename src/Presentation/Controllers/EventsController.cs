using EventManagement.Application.DTOs;
using EventManagement.Application.Filters;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace EventManagement.Presentation.Controllers
{
    /// <summary>
    /// Контроллер для работы с мероприятиями.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventsService _eventsService;

        /// <summary>
        /// Конструктор контролера мероприятий.
        /// </summary>
        /// <param name="eventsService">Сервис мероприятий.</param>
        public EventsController(IEventsService eventsService)
        {
            _eventsService = eventsService;
        }

        /// <summary>
        /// Создает новое мероприятие.
        /// </summary>
        /// <param name="addEventRequest">Данные для создания мероприятия.</param>
        /// <returns>Возвращает созданное мероприятие с кодом 201 (Created).</returns>
        /// <response code="201">Мероприятие успешно создано.</response>
        /// <response code="400">Некорректные данные запроса.</response>
        [HttpPost]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Add([FromBody] AddEventRequest addEventRequest)
        {
            var addedEvent = _eventsService.Add(addEventRequest);
            return CreatedAtAction(nameof(GetById), new { id = addedEvent.Id }, addedEvent);
        }

        /// <summary>
        /// Удаляет мероприятие по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <returns>Возвращает код 204 (No Content) при успешном удалении.</returns>
        /// <response code="204">Мероприятие успешно удалено.</response>
        /// <response code="404">Мероприятие с указанным id не найдено.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(Guid id)
        {
            _eventsService.Delete(id);
            return NoContent();
        }

        /// <summary>
        /// Получает список мероприятий с фильтрацией и пагинацией.
        /// </summary>
        /// <param name="title">Часть названия мероприятия (регистронезависимо).</param>
        /// <param name="from">Минимальная дата начала (включительно).</param>
        /// <param name="to">Максимальная дата окончания (включительно).</param>
        /// <param name="page">Номер страницы.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <returns>Возвращает данные текущей страницы с метаданными пагинации.</returns>
        /// <response code="200">Список мероприятий успешно получен.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResult<EventDto>), StatusCodes.Status200OK)]
        public ActionResult<PaginatedResult<EventDto>> GetAll(string? title, DateTime? from, DateTime? to, int? page = 1, int? pageSize = 10)
        {
            var eventFilter = new EventFilter
            {
                Title = title,
                StartAt = from,
                EndAt = to,
            };
            var events = _eventsService.Filter(eventFilter, page!.Value, pageSize!.Value);
            return Ok(events);
        }

        /// <summary>
        /// Получает список мероприятий по фильтрам из тела запроса.
        /// </summary>
        /// <param name="eventFilter">Фильтр для мероприятий.</param>
        /// <param name="page">Номер страницы.</param>
        /// <param name="pageSize">Размер страницы.</param>
        /// <returns>Возвращает данные текущей страницы с метаданными пагинации.</returns>
        /// <response code="200">Список мероприятий успешно получен.</response>
        [HttpPost("Filter")]
        [ProducesResponseType(typeof(PaginatedResult<EventDto>), StatusCodes.Status200OK)]
        public ActionResult<PaginatedResult<EventDto>> Filter([FromBody] EventFilter eventFilter, int? page = 1, int? pageSize = 10)
        {
            var events = _eventsService.Filter(eventFilter, page!.Value, pageSize!.Value);
            return Ok(events);
        }

        /// <summary>
        /// Получает мероприятие по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <returns>Возвращает мероприятие с кодом 200 (OK).</returns>
        /// <response code="200">Мероприятие успешно найдено.</response>
        /// <response code="404">Мероприятие с указанным id не найдено.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<EventDto> GetById(Guid id)
        {
            var eventItem = _eventsService.GetById(id);
            return Ok(eventItem);
        }

        /// <summary>
        /// Обновляет существующее мероприятие.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="updateEventRequest">Данные для обновления мероприятия.</param>
        /// <returns>Возвращает код 204 (No Content) при успешном обновлении.</returns>
        /// <response code="204">Мероприятие успешно обновлено.</response>
        /// <response code="400">Некорректные данные запроса.</response>
        /// <response code="404">Мероприятие с указанным id не найдено.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Update(Guid id, [FromBody] UpdateEventRequest updateEventRequest)
        {
            _eventsService.Update(id, updateEventRequest);
            return NoContent();
        }
    }
}
