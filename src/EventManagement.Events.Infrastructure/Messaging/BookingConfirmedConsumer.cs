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
            await Task.Yield();

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            consumer.Subscribe(new[] { KafkaTopics.BookingConfirmed, KafkaTopics.BookingCancelled });

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
                        await HandleMessageAsync(consumeResult.Topic, consumeResult.Message.Value, stoppingToken);
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

        /// <summary>
        /// Обрабатывает сообщение Kafka о подтверждении или отмене брони.
        /// </summary>
        /// <param name="topic">Имя топика.</param>
        /// <param name="payload">Тело сообщения.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        private async Task HandleMessageAsync(string topic, string payload, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            if (topic == KafkaTopics.BookingConfirmed)
            {
                var confirmedMessage = KafkaJsonSerializer.Deserialize<BookingConfirmedMessage>(payload);
                if (confirmedMessage == null)
                {
                    _logger.Warn("Не удалось десериализовать BookingConfirmedMessage");
                    return;
                }

                bool wasReserved = await eventService.TryReserveSeats(
                    confirmedMessage.EventId,
                    confirmedMessage.SeatsCount,
                    cancellationToken);
                if (!wasReserved)
                {
                    _logger.Warn(
                        "Не удалось зарезервировать места в Events. BookingId={0}, EventId={1}, SeatsCount={2}",
                        confirmedMessage.BookingId,
                        confirmedMessage.EventId,
                        confirmedMessage.SeatsCount);
                    return;
                }

                _logger.Debug(
                    "Места зарезервированы в Events по booking-confirmed. BookingId={0}, EventId={1}, ConfirmedAt={2}",
                    confirmedMessage.BookingId,
                    confirmedMessage.EventId,
                    confirmedMessage.ConfirmedAt);
                return;
            }

            if (topic == KafkaTopics.BookingCancelled)
            {
                var cancelledMessage = KafkaJsonSerializer.Deserialize<BookingCancelledMessage>(payload);
                if (cancelledMessage == null)
                {
                    _logger.Warn("Не удалось десериализовать BookingCancelledMessage");
                    return;
                }

                try
                {
                    await eventService.ReleaseSeats(
                        cancelledMessage.EventId,
                        cancelledMessage.SeatsCount,
                        cancellationToken);
                    _logger.Debug(
                        "Места освобождены в Events по booking-cancelled. BookingId={0}, EventId={1}",
                        cancelledMessage.BookingId,
                        cancelledMessage.EventId);
                }
                catch (Exception exception)
                {
                    _logger.Warn(
                        "Не удалось освободить места в Events. BookingId={0}, EventId={1}, SeatsCount={2}. Error={3}",
                        cancelledMessage.BookingId,
                        cancelledMessage.EventId,
                        cancelledMessage.SeatsCount,
                        exception.Message);
                }
                return;
            }

            _logger.Warn("Получено сообщение из неподдерживаемого топика {0}", topic);
        }
    }
}
