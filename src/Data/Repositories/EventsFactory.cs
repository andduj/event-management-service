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
                    Title = "Ревизор",
                    Description = "Спектакль. Постановка - Театр им. Ленсовета.",
                    StartAt = new DateTime(2026, 3, 3, 19, 0, 0),
                    EndAt = new DateTime(2026, 3, 3, 22, 20, 0),
                },
                new Event
                {
                    Id = Guid.NewGuid(),
                    Title = "Джазовый концерт",
                    Description = "Концерт",
                    StartAt = new DateTime(2026, 3, 2, 17, 0, 0),
                    EndAt = new DateTime(2026, 3, 2, 20, 0, 0),
                }
            };
        }
    }
}
