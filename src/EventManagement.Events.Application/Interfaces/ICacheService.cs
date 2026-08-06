using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Application.Interfaces
{
    /// <summary>
    /// Абстракция кеша приложения.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Получает значение по ключу.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="key">Ключ кеша.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Значение или <c>null</c>, если ключ отсутствует либо кеш недоступен.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Записывает значение с временем жизни.
        /// </summary>
        /// <typeparam name="T">Тип значения.</typeparam>
        /// <param name="key">Ключ кеша.</param>
        /// <param name="value">Значение.</param>
        /// <param name="ttl">Время жизни записи.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удаляет значение по ключу.
        /// </summary>
        /// <param name="key">Ключ кеша.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }
}
