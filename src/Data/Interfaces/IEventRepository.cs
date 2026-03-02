using EventManagement.Models;

namespace EventManagement.Data.Interfaces
{
    public interface IEventRepository
    {
        List<Event> GetAll();

        Event GetById(Guid id);

        Event Add(Event newEvent);

        void Update(Event updatedEvent);

        void Delete(Guid id);
    }
}
