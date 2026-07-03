using Confluent.Kafka;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Contracts.Events;
using EventManagement.Contracts.Kafka;
using EventManagement.Bookings.Infrastructure.Kafka;
using EventManagement.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure.Messaging
{
    /// <summary>
    /// Потребитель топиков жизненного цикла мероприятий из Events.
    /// </summary>
    public sealed class EventLifecycleConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventLifecycleConsumer> _logger;
        private readonly KafkaOptions _options;

        /// <summary>
        /// Инициализирует потребителя сообщений о мероприятиях.
        /// </summary>
        public EventLifecycleConsumer(
            IServiceScopeFactory scopeFactory,
            ILogger<EventLifecycleConsumer> logger,
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
            consumer.Subscribe(new[]
            {
                KafkaTopics.EventCreated,
                KafkaTopics.EventUpdated,
                KafkaTopics.EventDeleted
            });

            _logger.Info(
                "Kafka consumer запущен. GroupId={0}, Topics={1}",
                _options.GroupId,
                string.Join(", ", KafkaTopics.EventCreated, KafkaTopics.EventUpdated, KafkaTopics.EventDeleted));

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);
                        await HandleMessageAsync(consumeResult, stoppingToken);
                        consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException exception)
                    {
                        _logger.Error(exception, "Ошибка чтения сообщения из Kafka");
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            finally
            {
                consumer.Close();
                _logger.Info("Kafka consumer остановлен");
            }
        }

        private async Task HandleMessageAsync(ConsumeResult<string, string> consumeResult, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBookableEventRepository>();

            switch (consumeResult.Topic)
            {
                case KafkaTopics.EventCreated:
                    await HandleCreatedAsync(consumeResult.Message.Value, repository, cancellationToken);
                    break;
                case KafkaTopics.EventUpdated:
                    await HandleUpdatedAsync(consumeResult.Message.Value, repository, cancellationToken);
                    break;
                case KafkaTopics.EventDeleted:
                    await HandleDeletedAsync(consumeResult.Message.Value, repository, cancellationToken);
                    break;
                default:
                    _logger.Warn("Получено сообщение из неизвестного топика {0}", consumeResult.Topic);
                    break;
            }
        }

        private async Task HandleCreatedAsync(string payload, IBookableEventRepository bookableEventRepository, CancellationToken cancellationToken)
        {
            var message = KafkaJsonSerializer.Deserialize<EventCreatedMessage>(payload);
            if (message == null)
            {
                _logger.Warn("Не удалось десериализовать EventCreatedMessage");
                return;
            }

            var bookableEvent = MapToBookableEvent(message);
            await bookableEventRepository.UpsertAsync(bookableEvent, cancellationToken);
            _logger.Debug("Синхронизировано создание мероприятия EventId={0}", message.EventId);
        }

        private async Task HandleUpdatedAsync(string payload, IBookableEventRepository bookableEventRepository, CancellationToken cancellationToken)
        {
            var message = KafkaJsonSerializer.Deserialize<EventUpdatedMessage>(payload);
            if (message == null)
            {
                _logger.Warn("Не удалось десериализовать EventUpdatedMessage");
                return;
            }

            var bookableEvent = MapToBookableEvent(message);
            await bookableEventRepository.UpsertAsync(bookableEvent, cancellationToken);
            _logger.Debug("Синхронизировано обновление мероприятия EventId={0}", message.EventId);
        }

        private async Task HandleDeletedAsync(string payload, IBookableEventRepository bookableEventRepository, CancellationToken cancellationToken)
        {
            var message = KafkaJsonSerializer.Deserialize<EventDeletedMessage>(payload);
            if (message == null)
            {
                _logger.Warn("Не удалось десериализовать EventDeletedMessage");
                return;
            }

            await bookableEventRepository.DeleteAsync(message.EventId, cancellationToken);
            _logger.Debug("Синхронизировано удаление мероприятия EventId={0}", message.EventId);
        }

        private static BookableEvent MapToBookableEvent(EventCreatedMessage message)
        {
            return BookableEvent.Create(
                message.EventId,
                message.Title,
                message.Description,
                message.StartAtUtc,
                message.EndAtUtc,
                message.TotalSeats,
                message.AvailableSeats);
        }

        private static BookableEvent MapToBookableEvent(EventUpdatedMessage message)
        {
            return BookableEvent.Create(
                message.EventId,
                message.Title,
                message.Description,
                message.StartAtUtc,
                message.EndAtUtc,
                message.TotalSeats,
                message.AvailableSeats);
        }
    }
}
