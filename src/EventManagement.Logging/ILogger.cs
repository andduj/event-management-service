using System;

namespace EventManagement.Logging
{
    /// <summary>
    /// Абстракция логгера приложения.
    /// </summary>
    public interface ILogger<T>
    {
        /// <summary>
        /// Пишет сообщение уровня Trace.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Параметры шаблона.</param>
        void Trace(string message, params object[] args);

        /// <summary>
        /// Пишет сообщение уровня Debug.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Параметры шаблона.</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Пишет сообщение уровня Info.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Параметры шаблона.</param>
        void Info(string message, params object[] args);

        /// <summary>
        /// Пишет сообщение уровня Warn.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Параметры шаблона.</param>
        void Warn(string message, params object[] args);

        /// <summary>
        /// Пишет сообщение уровня Error.
        /// </summary>
        /// <param name="exception">Исключение.</param>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="args">Параметры шаблона.</param>
        void Error(Exception exception, string message, params object[] args);
    }
}
