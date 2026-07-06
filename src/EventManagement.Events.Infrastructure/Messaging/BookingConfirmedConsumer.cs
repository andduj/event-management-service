using Confluent.Kafka;
using EventManagement.Contracts.Bookings;
using EventManagement.Contracts.Kafka;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Infrastructure.Kafka;
using EventManagement.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Infrastructure.Messaging
{
    /// <summary>
    /// Потребитель сообщений о подтверждённых бронях из Bookings.
    /// </summary>
    public sealed class BookingConfirmedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingConfirmedConsumer> _logger;
        private readonly KafkaOptions _options;

        /// <summary>
        /// Инициализирует потребителя сообщений booking-confirmed.
        /// </summary>
        public BookingConfirmedConsumer(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingConfirmedConsumer> logger,
            IOptions<KafkaOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(KafkaTopics.BookingConfirmed);

            _logger.Info(
                "Kafka consumer booking-confirmed запущен. GroupId={0}",
                _options.GroupId);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);
                        await HandleMessageAsync(consumeResult.Message.Value, stoppingToken);
                        consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException exception)
                    {
                        _logger.Error(exception, "Ошибка чтения booking-confirmed из Kafka");
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            finally
            {
                consumer.Close();
                _logger.Info("Kafka consumer booking-confirmed остановлен");
            }
        }

        private async Task HandleMessageAsync(string payload, CancellationToken cancellationToken)
        {
            var message = KafkaJsonSerializer.Deserialize<BookingConfirmedMessage>(payload);
            if (message == null)
            {
                _logger.Warn("Не удалось десериализовать BookingConfirmedMessage");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            bool wasReserved = await eventService.TryReserveSeats(message.EventId, message.SeatsCount);
            if (!wasReserved)
            {
                _logger.Warn(
                    "Не удалось зарезервировать места в Events. BookingId={0}, EventId={1}, SeatsCount={2}",
                    message.BookingId,
                    message.EventId,
                    message.SeatsCount);
                return;
            }

            _logger.Debug(
                "Места зарезервированы в Events по booking-confirmed. BookingId={0}, EventId={1}",
                message.BookingId,
                message.EventId);
        }
    }
}
