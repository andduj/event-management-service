using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        public BookingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("Сервис обработки бронирований начал работу");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var bookingProcessingService = scope.ServiceProvider.GetRequiredService<IBookingProcessingService>();
                    await bookingProcessingService.ProcessPendingBookingsAsync(stoppingToken);
                }
            }            
            finally
            {
                _logger.Info("Сервис обработки бронирований завершает работу");
            }
        }
    }
}
