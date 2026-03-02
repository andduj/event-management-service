using EventManagement.Data.Interfaces;
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
            var existing = _events.FirstOrDefault(e => e.Id == id);
            if(existing == null)
            {
                return;
            }

            _events.Remove(existing);
        }

        public List<Event> GetAll()
        {
            return _events;
        }

        public Event GetById(Guid id)
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }

        public void Update(Event updatedEvent)
        {
            var existing = _events.FirstOrDefault(e => e.Id == updatedEvent.Id);
            if (existing == null)
            {
                throw new ArgumentException();
            }

            existing.Title = updatedEvent.Title;
            existing.Description = updatedEvent.Description;
            existing.StartAt = updatedEvent.StartAt;
            existing.EndAt = updatedEvent.EndAt;
        }
    }
}
