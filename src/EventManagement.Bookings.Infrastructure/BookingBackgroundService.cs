using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
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
            _logger.Info("Сервис обработки бронирований начал работу (интервал опроса: {PollingIntervalSeconds} с)", _pollingInterval.TotalSeconds);

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
            catch (Exception exception)
            {
                _logger.Error(exception, "При обработки бронирований возникла ошибка");
            }
            finally
            {
                _logger.Info("Сервис обработки бронирований завершает работу");
            }
        }

        private async Task PollAndProcessPendingBookingsAsync(CancellationToken cancellationToken)
        {
            var pendingBookingIds = await GetPendingBookingIdsAsync(cancellationToken);
            if (pendingBookingIds.Count == 0)
            {
                return;
            }

            var tasks = pendingBookingIds.Select(bookingId => ProcessBookingInScopeAsync(bookingId, cancellationToken));
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
            using var scope = _scopeFactory.CreateScope();
            var bookingProcessingService = scope.ServiceProvider.GetRequiredService<IBookingProcessingService>();
            await bookingProcessingService.ProcessBookingAsync(bookingId, cancellationToken);
        }
    }
}
