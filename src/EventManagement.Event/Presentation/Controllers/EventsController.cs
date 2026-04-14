using EventManagement.Events.Application.DTOs;
using EventManagement.Events.Application.Filters;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Application.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Presentation.Controllers
{
    /// <summary>
    /// Контроллер для работы с мероприятиями.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        /// <summary>
        /// Конструктор контролера мероприятий.
        /// </summary>
        /// <param name="EventService">Сервис мероприятий.</param>
        public EventsController(IEventService EventService)
        {
            _eventService = EventService;
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
        public async Task<ActionResult> CreateEventAsync([FromBody] AddEventRequest addEventRequest)
        {
            var addedEvent = await _eventService.CreateEventAsync(addEventRequest);
            return CreatedAtAction(nameof(GetEventByIdAsync), new { id = addedEvent.Id }, addedEvent);
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
        public async Task<ActionResult> DeleteEventAsync(Guid id)
        {
            await _eventService.DeleteEventAsync(id);
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
        public async Task<ActionResult<PaginatedResult<EventDto>>> GetEventsAsync(string? title, DateTime? from, DateTime? to, int? page = 1, int? pageSize = 10)
        {
            var eventFilter = new EventFilter
            {
                Title = title,
                StartAt = from,
                EndAt = to,
            };
            var events = await _eventService.FilterAsync(eventFilter, page!.Value, pageSize!.Value);
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
        [HttpPost("filter")]
        [ProducesResponseType(typeof(PaginatedResult<EventDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<EventDto>>> FilterEventsAsync([FromBody] EventFilter eventFilter, int? page = 1, int? pageSize = 10)
        {
            var events = await _eventService.FilterAsync(eventFilter, page!.Value, pageSize!.Value);
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
        public async Task<ActionResult<EventDto>> GetEventByIdAsync(Guid id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
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
        public async Task<ActionResult> UpdateEventAsync(Guid id, [FromBody] UpdateEventRequest updateEventRequest)
        {
            await _eventService.UpdateEventAsync(id, updateEventRequest);
            return NoContent();
        }

        /// <summary>
        /// Пытается зарезервировать указанное число мест на мероприятии.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест для резервирования (по умолчанию 1).</param>
        /// <returns>
        /// <c>true</c>, если места успешно зарезервированы; <c>false</c>, если мест недостаточно или передано некорректное число.
        /// </returns>
        /// <response code="200">Результат попытки резервирования.</response>
        /// <response code="404">Мероприятие с указанным id не найдено.</response>
        [HttpPost("{id}/reserve-seats")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> TryReserveSeats([FromRoute] Guid id, [FromQuery] int count = 1)
        {
            var wasReserved = await _eventService.TryReserveSeats(id, count);
            return Ok(wasReserved);
        }

        /// <summary>
        /// Освобождает указанное количество мест на мероприятии.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="count">Количество мест для освобождения (по умолчанию 1).</param>
        /// <returns>Возвращает код 204 (No Content) при успешном освобождении мест.</returns>
        /// <response code="204">Места успешно освобождены.</response>
        /// <response code="404">Мероприятие с указанным id не найдено.</response>
        [HttpPost("{id}/release-seats")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ReleaseSeats([FromRoute] Guid id, [FromQuery] int count = 1)
        {
            await _eventService.ReleaseSeats(id, count);
            return NoContent();
        }

        /// <summary>
        /// Проверяет существование мероприятия по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><c>true</c>, если мероприятие существует; иначе <c>false</c>.</returns>
        /// <response code="200">Результат проверки существования мероприятия.</response>
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> Exists([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            bool exists = await _eventService.Exists(id, cancellationToken);
            return Ok(exists);
        }
    }
}
