using EventManagement.Auth.Application.DTOs;
using EventManagement.Auth.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Auth.Application.Interfaces
{
    /// <summary>
    /// Сервис регистрации и аутентификации пользователей.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Регистрирует нового пользователя.
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="password">Пароль.</param>
        /// <param name="role">Роль пользователя.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task RegisterAsync(
            string login,
            string password,
            UserRole role = UserRole.User,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполняет вход и возвращает JWT-токен.
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="password">Пароль.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Данные аутентификации с токеном.</returns>
        Task<AuthTokenDto> LoginAsync(
            string login,
            string password,
            CancellationToken cancellationToken = default);
    }
}
