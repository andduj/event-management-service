namespace EventManagement.Auth.Application.DTOs
{
    /// <summary>
    /// Запрос на вход в систему.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public required string Login { get; set; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        public required string Password { get; set; }
    }
}
