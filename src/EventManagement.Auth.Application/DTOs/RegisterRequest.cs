using EventManagement.Auth.Domain.Models;

namespace EventManagement.Auth.Application.DTOs
{
    /// <summary>
    /// Запрос на регистрацию пользователя.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public required string Login { get; set; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Роль пользователя (по умолчанию User).
        /// </summary>
        public UserRole? Role { get; set; }
    }
}
