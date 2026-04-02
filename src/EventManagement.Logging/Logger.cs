using NLog;
using System;

namespace EventManagement.Logging
{
    /// <summary>
    /// Обертка над NLog для логирования в приложении.
    /// </summary>
    /// <typeparam name="T">Тип владельца логгера.</typeparam>
    public sealed class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logger = LogManager.GetLogger(typeof(T).FullName ?? typeof(T).Name);

        /// <inheritdoc />
        public void Trace(string message, params object[] args)
        {
            _logger.Trace(message, args);
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        /// <inheritdoc />
        public void Error(Exception exception, string message, params object[] args)
        {
            _logger.Error(exception, message, args);
        }
    }
}
