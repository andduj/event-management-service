namespace EventManagement.Bookings.Domain.Models
{
    /// <summary>
    /// Статус бронирования.
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Ожидает обработки.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Подтверждена.
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// Отклонена.
        /// </summary>
        Rejected = 2,
    }
}
