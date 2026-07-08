using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventManagement.Contracts.Kafka;
using EventManagement.Events.Infrastructure.Kafka;
using EventManagement.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Events.Infrastructure.Messaging
{
    /// <summary>
    /// Инициализирует Kafka-топики, необходимые сервису Events.
    /// </summary>
    public sealed class KafkaTopicInitializer : IHostedService
    {
        private readonly KafkaOptions _options;
        private readonly ILogger<KafkaTopicInitializer> _logger;

        public KafkaTopicInitializer(
            IOptions<KafkaOptions> options,
            ILogger<KafkaTopicInitializer> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var adminConfig = new AdminClientConfig
                {
                    BootstrapServers = _options.BootstrapServers
                };

                using var adminClient = new AdminClientBuilder(adminConfig).Build();
                await adminClient.CreateTopicsAsync(
                    new[]
                    {
                        new TopicSpecification
                        {
                            Name = KafkaTopics.BookingConfirmed,
                            NumPartitions = 1,
                            ReplicationFactor = 1
                        },
                        new TopicSpecification
                        {
                            Name = KafkaTopics.BookingCancelled,
                            NumPartitions = 1,
                            ReplicationFactor = 1
                        }
                    });

                _logger.Info("Kafka-топики инициализированы");
            }
            catch (CreateTopicsException exception)
            {
                bool onlyAlreadyExists = true;
                foreach (var result in exception.Results)
                {
                    if (result.Error.Code != ErrorCode.TopicAlreadyExists)
                    {
                        onlyAlreadyExists = false;
                        break;
                    }
                }

                if (onlyAlreadyExists)
                {
                    _logger.Info("Kafka-топики уже существуют");
                    return;
                }

                _logger.Error(exception, "Не удалось создать Kafka-топики");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Ошибка инициализации Kafka-топиков");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
