using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure
{
    /// <summary>
    /// Фоновая обработка бронирований.
    /// </summary>
    public class BookingBackgroundService : BackgroundService
    {
        private const int IntervalInMilliseconds = 5000;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр фонового сервиса обработки бронирований.
        /// </summary>
        /// <param name="scopeFactory">Фабрика скоупов DI-контейнера.</param>
        /// <param name="logger">Логгер приложения.</param>
        public BookingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Запускает цикл фоновой обработки бронирований.
        /// </summary>
        /// <param name="cancellationToken">Токен остановки фонового сервиса.</param>
        /// <returns>Задача выполнения фонового сервиса.</returns>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.Info("Сервис обработки бронирований начал работу");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var bookingProcessingService = scope.ServiceProvider.GetRequiredService<IBookingProcessingService>();
                    await bookingProcessingService.ProcessPendingBookingsAsync(cancellationToken);
                    await Task.Delay(IntervalInMilliseconds, cancellationToken);
                }
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
    }
}
