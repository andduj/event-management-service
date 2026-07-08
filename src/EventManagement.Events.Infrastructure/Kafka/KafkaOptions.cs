namespace EventManagement.Events.Infrastructure.Kafka
{
    /// <summary>
    /// Параметры подключения к Kafka.
    /// </summary>
    public sealed class KafkaOptions
    {
        /// <summary>
        /// Имя секции конфигурации.
        /// </summary>
        public const string SectionName = "Kafka";

        /// <summary>
        /// Адреса брокеров Kafka.
        /// </summary>
        public string BootstrapServers { get; set; } = "localhost:9092";

        /// <summary>
        /// Идентификатор consumer group.
        /// </summary>
        public string GroupId { get; set; } = "events-service";
    }
}
