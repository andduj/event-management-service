using EventManagement.Events.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Infrastructure.Redis
{
    /// <summary>
    /// Реализация кеша на Redis.
    /// При недоступности Redis ошибки логируются, наружу не пробрасываются.
    /// </summary>
    public sealed class RedisCacheService : ICacheService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RedisCacheService> _logger;

        /// <summary>
        /// Инициализирует сервис кеша Redis.
        /// </summary>
        /// <param name="connectionMultiplexer">Соединение с Redis.</param>
        /// <param name="logger">Логгер.</param>
        public RedisCacheService(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisCacheService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                IDatabase database = _connectionMultiplexer.GetDatabase();
                RedisValue value = await database.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(value!, SerializerOptions);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Ошибка чтения из Redis. Key={0}", key);
                return default;
            }
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                IDatabase database = _connectionMultiplexer.GetDatabase();
                string payload = JsonSerializer.Serialize(value, SerializerOptions);
                await database.StringSetAsync(key, payload, ttl);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Ошибка записи в Redis. Key={0}", key);
            }
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                IDatabase database = _connectionMultiplexer.GetDatabase();
                await database.KeyDeleteAsync(key);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Ошибка удаления из Redis. Key={0}", key);
            }
        }
    }
}
