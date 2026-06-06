namespace EventManagement.Bookings.Infrastructure
{
    /// <summary>
    /// Параметры фоновой обработки бронирований.
    /// </summary>
    public class BookingProcessingOptions
    {
        /// <summary>
        /// Имя секции в <c>appsettings</c>.
        /// </summary>
        public const string SectionName = "BookingProcessing";

        /// <summary>
        /// Интервал между циклами опроса очереди (секунды).
        /// </summary>
        public int PollingIntervalSeconds { get; set; } = 5;
    }
}
