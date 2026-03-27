using EventManagement.Bookings.Application.Services;
using EventManagement.Bookings.Data.Interfaces;
using EventManagement.Bookings.Models;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Logging.ILogger<BookingService> _logger;

        public BookingBackgroundService(IServiceScopeFactory scopeFactory, Logging.ILogger<BookingService> logger)
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
                    await Task.Delay(5000, stoppingToken);

                    using var scope = _scopeFactory.CreateScope();
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    var bookings = await bookingRepository.GetBookingsAsync(BookingStatus.Pending);

                    foreach (var booking in bookings)
                    {
                        try
                        {
                            await Task.Delay(2000, stoppingToken);
                            booking.Status = BookingStatus.Confirmed;
                            booking.ProcessedAt = DateTime.UtcNow;
                            await bookingRepository.UpdateBookingAsync(booking);
                        }
                        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                        {
                            return;
                        }
                        catch (Exception exception)
                        {
                            _logger.Error(exception, $"Ошибка при обработке бронирования {booking.Id}");
                        }
                    }
                }
            }            
            finally
            {
                _logger.Info("Сервис обработки бронирований завершает работу");
            }
        }
    }
}
