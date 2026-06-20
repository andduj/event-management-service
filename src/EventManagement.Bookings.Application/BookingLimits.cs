namespace EventManagement.Bookings.Application
{
    /// <summary>
    /// Ограничения для бронирований.
    /// </summary>
    public static class BookingLimits
    {
        /// <summary>
        /// Максимальное количество активных бронирований на одного пользователя.
        /// </summary>
        public const int MaxActiveBookings = 10;
    }
}
