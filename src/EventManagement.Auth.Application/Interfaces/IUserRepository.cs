using EventManagement.Auth.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Auth.Application.Interfaces
{
    /// <summary>
    /// Репозиторий пользователей.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Сохраняет нового пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Сохранённый пользователь.</returns>
        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ищет пользователя по логину.
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Пользователь или <c>null</c>, если не найден.</returns>
        Task<User?> FindByLoginAsync(string login, CancellationToken cancellationToken = default);

        /// <summary>
        /// Проверяет, существует ли пользователь с указанным логином.
        /// </summary>
        /// <param name="login">Логин.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns><c>true</c>, если логин уже занят.</returns>
        Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken = default);
    }
}
