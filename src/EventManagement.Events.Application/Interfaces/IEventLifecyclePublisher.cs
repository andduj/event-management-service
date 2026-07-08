using EventManagement.Events.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Application.Interfaces
{
    /// <summary>
    /// Публикация событий жизненного цикла мероприятия в Kafka.
    /// </summary>
    public interface IEventLifecyclePublisher
    {
        /// <summary>
        /// Публикует сообщение о создании мероприятия.
        /// </summary>
        /// <param name="eventItem">Созданное мероприятие.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task PublishCreatedAsync(Event eventItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Публикует сообщение об обновлении мероприятия.
        /// </summary>
        /// <param name="eventItem">Обновлённое мероприятие.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task PublishUpdatedAsync(Event eventItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Публикует сообщение об удалении мероприятия.
        /// </summary>
        /// <param name="eventId">Идентификатор удалённого мероприятия.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task PublishDeletedAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
