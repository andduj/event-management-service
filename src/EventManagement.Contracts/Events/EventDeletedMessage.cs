using System;

namespace EventManagement.Contracts.Events
{
    /// <summary>
    /// Сообщение об удалении мероприятия для синхронизации в Bookings.
    /// </summary>
    public sealed class EventDeletedMessage
    {
        /// <summary>
        /// Идентификатор удалённого мероприятия.
        /// </summary>
        public required Guid EventId { get; init; }
    }
}
