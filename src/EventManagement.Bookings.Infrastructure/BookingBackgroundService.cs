using EventManagement.Bookings.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure
{
    /// <summary>
    /// Фоновая обработка бронирований.
    /// </summary>
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly TimeSpan _pollingInterval;
        private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _processingLocks = new();

        /// <summary>
        /// Инициализирует новый экземпляр фонового сервиса обработки бронирований.
        /// </summary>
        /// <param name="scopeFactory">Фабрика скоупов DI-контейнера.</param>
        /// <param name="logger">Логгер приложения.</param>
        /// <param name="options">Параметры интервала опроса очереди.</param>
        public BookingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingBackgroundService> logger,
            IOptions<BookingProcessingOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _pollingInterval = TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds);
        }

        /// <summary>
        /// Запускает цикл фоновой обработки бронирований.
        /// </summary>
        /// <param name="cancellationToken">Токен остановки фонового сервиса.</param>
        /// <returns>Задача выполнения фонового сервиса.</returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Сервис обработки бронирований начал работу (интервал опроса: {PollingIntervalSeconds} с)", _pollingInterval.TotalSeconds);

            using var timer = new PeriodicTimer(_pollingInterval);

            try
            {
                do
                {
                    await PollAndProcessPendingBookingsAsync(cancellationToken);
                }
                while (await timer.WaitForNextTickAsync(cancellationToken));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {

            }
            finally
            {
                _logger.LogInformation("Сервис обработки бронирований завершает работу");
            }
        }

        private async Task PollAndProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            List<Guid> pendingBookingIds;
            try
            {
                pendingBookingIds = await GetPendingBookingIdsAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось получить список ожидающих бронирований");
                return;
            }

            var uniquePendingBookingIds = pendingBookingIds.Distinct().ToList();
            if (uniquePendingBookingIds.Count == 0)
            {
                return;
            }

            var tasks = uniquePendingBookingIds.Select(bookingId => ProcessBookingInScopeAsync(bookingId, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task<List<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingProcessingService = scope.ServiceProvider.GetRequiredService<IBookingProcessingService>();
            return await bookingProcessingService.GetPendingBookingIdsAsync(cancellationToken);
        }

        private async Task ProcessBookingInScopeAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var processingLock = _processingLocks.GetOrAdd(bookingId, _ => new SemaphoreSlim(1, 1));
            if (!await processingLock.WaitAsync(0, cancellationToken))
            {
                _logger.LogDebug("Бронирование {0} уже обрабатывается в текущем экземпляре сервиса", bookingId);
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var bookingProcessingService = scope.ServiceProvider.GetRequiredService<IBookingProcessingService>();
                await bookingProcessingService.ProcessBookingAsync(bookingId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ошибка при обработке бронирования {0}", bookingId);
            }
            finally
            {
                processingLock.Release();
                if (processingLock.CurrentCount == 1)
                {
                    _processingLocks.TryRemove(bookingId, out _);
                }
            }
        }
    }
}
