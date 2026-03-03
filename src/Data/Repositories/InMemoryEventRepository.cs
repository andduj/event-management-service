using EventManagement.Data.Interfaces;
using EventManagement.Exceptions;
using EventManagement.Models;

namespace EventManagement.Data.Repositories
{
    public class InMemoryEventRepository : IEventRepository
    {
        private static readonly List<Event> _events;

        static InMemoryEventRepository() 
        {
            _events = EventsFactory.Create();
        }

        public Event Add(Event newEvent)
        {
            _events.Add(newEvent);
            return newEvent;
        }

        public void Delete(Guid id)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == id);
            if(eventItem == null)
            {
                throw new EventNotFoundException($"Событие с id={id} не найдено");
            }

            _events.Remove(eventItem);
        }

        public List<Event> GetAll()
        {
            return _events;
        }

        public Event GetById(Guid id)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == id);
            if(eventItem == null)
            {
                throw new EventNotFoundException($"Событие с id={id} не найдено");
            }
            return eventItem;
        }

        public void Update(Event updatedEvent)
        {
            var eventItem = _events.FirstOrDefault(e => e.Id == updatedEvent.Id);
            if (eventItem == null)
            {
                throw new EventNotFoundException($"Событие с id={updatedEvent.Id} не найдено");
            }

            eventItem.Title = updatedEvent.Title;
            eventItem.Description = updatedEvent.Description;
            eventItem.StartAt = updatedEvent.StartAt;
            eventItem.EndAt = updatedEvent.EndAt;
        }
    }
}
