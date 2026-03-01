using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers
{
    /// <summary>
    /// Контролер для работы с мероприятиями
    /// </summary>
    public class EventsControler : ControllerBase
    {
        public readonly IEventService _eventService;

        public EventsControler(IEventService eventService)
        {
            _eventService = eventService;
        }
    }
}
