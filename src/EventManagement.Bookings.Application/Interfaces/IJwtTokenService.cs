using EventManagement.Bookings.Domain.Models;

namespace EventManagement.Bookings.Application.Interfaces
{
    /// <summary>
    /// Сервис генерации JWT-токенов.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Формирует подписанный JWT-токен для указанного пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <returns>Строка JWT-токена.</returns>
        string GenerateToken(User user);
    }
}
