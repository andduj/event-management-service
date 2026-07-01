namespace EventManagement.Contracts.Kafka
{
    /// <summary>
    /// Имена Kafka-топиков для обмена сообщениями между сервисами.
    /// </summary>
    public static class KafkaTopics
    {
        /// <summary>
        /// Событие создано в Events (Events → Bookings).
        /// </summary>
        public const string EventCreated = "event-created";

        /// <summary>
        /// Событие обновлено в Events (Events → Bookings).
        /// </summary>
        public const string EventUpdated = "event-updated";

        /// <summary>
        /// Событие удалено в Events (Events → Bookings).
        /// </summary>
        public const string EventDeleted = "event-deleted";

        /// <summary>
        /// Бронь подтверждена в Bookings (Bookings → Events).
        /// </summary>
        public const string BookingConfirmed = "booking-confirmed";
    }
}
