using EventManagement.Models;

namespace EventManagement.Data.Repositories
{
    public static class EventsFactory
    {
        public static List<Event> Create()
        {
            return new List<Event>
            {
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "",
                    Description = "",
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "",
                    Description = "",
                    StartAt = DateTime.Now,
                    EndAt = DateTime.Now
                }
            };
        }
    }
}
