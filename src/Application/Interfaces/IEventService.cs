using EventManagement.Application.DTOs;
using EventManagement.Application.Requests;

namespace EventManagement.Application.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с мероприятиями
    /// </summary>
    public interface IEventService
    {
        List<EventDto> GetAll();

        EventDto GetById(Guid id);

        EventDto Add(AddEventRequest addEventRequest);

        void Update(Guid id, UpdateEventRequest updateEventRequest);

        void Delete(Guid id);
    }
}
