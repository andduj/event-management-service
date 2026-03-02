using EventManagement.Application.DTOs;
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
    public class EventsControler : ControllerBase
    {
        public readonly IEventService _eventService;

        public EventsControler(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost]
        public ActionResult Add([FromBody] AddEventRequest addEventRequest)
        {
            var addedEvent = _eventService.Add(addEventRequest);
            return CreatedAtAction(nameof(GetById), new { id = addedEvent.Id }, addEventRequest);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            var eventItem = _eventService.GetById(id);
            if (eventItem == null)
            {
                return NotFound($"Событие с id={id} не найдено");
            }
            _eventService.Delete(id);
            return NoContent();
        }

        [HttpGet]
        public ActionResult<List<EventDto>> GetAll()
        {
            var events = _eventService.GetAll();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public ActionResult<EventDto> GetById(Guid id)
        {
            var eventItem = _eventService.GetById(id);
            if (eventItem == null)
            {
                return NotFound($"Событие с id={id} не найдено");
            }

            return Ok(eventItem);
        }

        [HttpPut("{id}")]
        public ActionResult Update(Guid id, [FromBody] UpdateEventRequest updateEventRequest)
        {
            var existingEvent = _eventService.GetById(id);
            if (existingEvent == null)
            {
                return NotFound($"Событие с id={id} не найдено");
            }

            _eventService.Update(id, updateEventRequest);

            return NoContent();
        }
    }
}
