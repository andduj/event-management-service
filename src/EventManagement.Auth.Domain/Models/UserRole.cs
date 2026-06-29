namespace EventManagement.Auth.Domain.Models
{
    /// <summary>
    /// Роль пользователя в системе.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Обычный пользователь: бронирование и отмена собственных броней.
        /// </summary>
        User = 0,

        /// <summary>
        /// Администратор: управление событиями и отмена любых броней.
        /// </summary>
        Admin = 1,
    }
}
