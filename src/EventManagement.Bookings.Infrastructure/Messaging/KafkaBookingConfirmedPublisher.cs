using Confluent.Kafka;
using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Models;
using EventManagement.Bookings.Infrastructure.Kafka;
using EventManagement.Contracts.Bookings;
using EventManagement.Contracts.Kafka;
using EventManagement.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure.Messaging
{
    /// <summary>
    /// Публикация событий о подтверждении и отмене брони в Kafka.
    /// </summary>
    public sealed class KafkaBookingConfirmedPublisher : IBookingConfirmedPublisher, IDisposable
    {
        private const int DefaultSeatsPerBooking = 1;

        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaBookingConfirmedPublisher> _logger;

        /// <summary>
        /// Инициализирует издателя Kafka-сообщений по бронированиям.
        /// </summary>
        public KafkaBookingConfirmedPublisher(
            IOptions<KafkaOptions> options,
            ILogger<KafkaBookingConfirmedPublisher> logger)
        {
            _logger = logger;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers
            };
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        /// <inheritdoc />
        public async Task PublishConfirmedAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = new BookingConfirmedMessage
            {
                BookingId = booking.Id,
                EventId = booking.EventId,
                UserId = booking.UserId,
                SeatsCount = DefaultSeatsPerBooking,
                ConfirmedAt = booking.ProcessedAt ?? DateTime.UtcNow
            };

            string payload = KafkaJsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = booking.EventId.ToString(),
                Value = payload
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(
                    KafkaTopics.BookingConfirmed,
                    kafkaMessage,
                    cancellationToken);
                _logger.Debug(
                    "Сообщение booking-confirmed опубликовано. BookingId={0}, Partition={1}, Offset={2}",
                    booking.Id,
                    deliveryResult.Partition.Value,
                    deliveryResult.Offset.Value);
            }
            catch (ProduceException<string, string> exception)
            {
                _logger.Error(exception, "Не удалось опубликовать booking-confirmed. BookingId={0}", booking.Id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task PublishCancelledAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = new BookingCancelledMessage
            {
                BookingId = booking.Id,
                EventId = booking.EventId,
                UserId = booking.UserId,
                SeatsCount = DefaultSeatsPerBooking
            };

            string payload = KafkaJsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = booking.EventId.ToString(),
                Value = payload
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(
                    KafkaTopics.BookingCancelled,
                    kafkaMessage,
                    cancellationToken);
                _logger.Debug(
                    "Сообщение booking-cancelled опубликовано. BookingId={0}, Partition={1}, Offset={2}",
                    booking.Id,
                    deliveryResult.Partition.Value,
                    deliveryResult.Offset.Value);
            }
            catch (ProduceException<string, string> exception)
            {
                _logger.Error(exception, "Не удалось опубликовать booking-cancelled. BookingId={0}", booking.Id);
                throw;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
