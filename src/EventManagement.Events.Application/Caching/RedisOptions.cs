namespace EventManagement.Events.Application.Caching
{
    /// <summary>
    /// Параметры подключения к Redis и TTL кеша.
    /// </summary>
    public sealed class RedisOptions
    {
        /// <summary>
        /// Имя секции конфигурации.
        /// </summary>
        public const string SectionName = "Redis";

        /// <summary>
        /// Строка подключения к Redis (host:port).
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        /// TTL кеша события по идентификатору, секунды.
        /// </summary>
        public int EventTtlSeconds { get; set; } = 300;

        /// <summary>
        /// TTL кеша топ-10 популярных событий, секунды.
        /// </summary>
        public int Top10TtlSeconds { get; set; } = 60;
    }
}
