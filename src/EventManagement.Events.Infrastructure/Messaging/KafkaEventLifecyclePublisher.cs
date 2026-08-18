using Confluent.Kafka;
using EventManagement.Contracts.Events;
using EventManagement.Contracts.Kafka;
using EventManagement.Events.Application.Interfaces;
using EventManagement.Events.Domain.Models;
using EventManagement.Events.Infrastructure.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Infrastructure.Messaging
{
    /// <summary>
    /// Публикация событий жизненного цикла мероприятия в Kafka.
    /// </summary>
    public sealed class KafkaEventLifecyclePublisher : IEventLifecyclePublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaEventLifecyclePublisher> _logger;

        /// <summary>
        /// Инициализирует издателя сообщений о мероприятиях.
        /// </summary>
        public KafkaEventLifecyclePublisher(
            IOptions<KafkaOptions> options,
            ILogger<KafkaEventLifecyclePublisher> logger)
        {
            _logger = logger;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers
            };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        /// <inheritdoc />
        public async Task PublishCreatedAsync(Event eventItem, CancellationToken cancellationToken = default)
        {
            var message = MapToCreatedMessage(eventItem);
            await PublishAsync(KafkaTopics.EventCreated, eventItem.Id.ToString(), message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task PublishUpdatedAsync(Event eventItem, CancellationToken cancellationToken = default)
        {
            var message = MapToUpdatedMessage(eventItem);
            await PublishAsync(KafkaTopics.EventUpdated, eventItem.Id.ToString(), message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task PublishDeletedAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var message = new EventDeletedMessage { EventId = eventId };
            await PublishAsync(KafkaTopics.EventDeleted, eventId.ToString(), message, cancellationToken);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _producer.Dispose();
        }

        private async Task PublishAsync<T>(string topic, string key, T message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string payload = KafkaJsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = payload
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
                _logger.LogDebug(
                    "Сообщение опубликовано в Kafka. Topic={0}, Key={1}, Partition={2}, Offset={3}",
                    deliveryResult.Topic,
                    deliveryResult.Message.Key,
                    deliveryResult.Partition.Value,
                    deliveryResult.Offset.Value);
            }
            catch (ProduceException<string, string> exception)
            {
                _logger.LogError(exception, "Не удалось опубликовать сообщение в Kafka. Topic={0}, Key={1}", topic, key);
                throw;
            }
        }

        private static EventCreatedMessage MapToCreatedMessage(Event eventItem)
        {
            return new EventCreatedMessage
            {
                EventId = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                StartAtUtc = eventItem.StartAt,
                EndAtUtc = eventItem.EndAt,
                TotalSeats = eventItem.TotalSeats,
                AvailableSeats = eventItem.AvailableSeats
            };
        }

        private static EventUpdatedMessage MapToUpdatedMessage(Event eventItem)
        {
            return new EventUpdatedMessage
            {
                EventId = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                StartAtUtc = eventItem.StartAt,
                EndAtUtc = eventItem.EndAt,
                TotalSeats = eventItem.TotalSeats,
                AvailableSeats = eventItem.AvailableSeats
            };
        }
    }
}
