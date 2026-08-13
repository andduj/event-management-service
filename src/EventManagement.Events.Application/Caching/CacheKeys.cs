using System;

namespace EventManagement.Events.Application.Caching
{
    /// <summary>
    /// Ключи кеша сервиса событий.
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// Ключ кеша топ-10 популярных событий.
        /// </summary>
        public const string Top10Events = "events:top10";

        /// <summary>
        /// Формирует ключ кеша события по идентификатору.
        /// </summary>
        /// <param name="eventId">Идентификатор события.</param>
        /// <returns>Ключ вида <c>event:{id}</c>.</returns>
        public static string EventById(Guid eventId) => $"event:{eventId}";
    }
}
