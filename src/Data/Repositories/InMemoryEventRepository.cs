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
    }
}
