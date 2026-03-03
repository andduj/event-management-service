using EventManagement.Models;

namespace EventManagement.Data.Interfaces
{
    /// <summary>
    /// Репозиторий для управления мероприятиями.
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Получает список всех мероприятий из репозитория.
        /// </summary>
        /// <returns>Список всех мероприятий.</returns>
        List<Event> GetAll();

        /// <summary>
        /// Получает конкретное мероприятие по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия (GUID).</param>
        /// <returns>Мероприятие, если найдено; иначе null.</returns>
        Event GetById(Guid id);

        /// <summary>
        /// Добавляет новое мероприятие в репозиторий.
        /// </summary>
        /// <param name="newEvent">Объект мероприятия для добавления.</param>
        /// <returns>Добавленное мероприятие с сгенерированными данными (например, ID).</returns>
        Event Add(Event newEvent);

        /// <summary>
        /// Обновляет существующее мероприятие в репозитории.
        /// </summary>
        /// <param name="updatedEvent">Объект мероприятия с обновленными данными.</param>
        void Update(Event updatedEvent);

        /// <summary>
        /// Удаляет мероприятие из репозитория по его идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор мероприятия для удаления (GUID).</param>
        void Delete(Guid id);
    }
}
