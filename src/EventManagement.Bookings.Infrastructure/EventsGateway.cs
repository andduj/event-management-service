using EventManagement.Bookings.Application.Interfaces;
using EventManagement.Bookings.Domain.Exceptions;
using EventManagement.Events.Api;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventManagement.Bookings.Infrastructure
{
    /// <summary>
    /// Адаптер NSwag-клиента Events API к порту <see cref="IEventsGateway"/>.
    /// </summary>
    public class EventsGateway : IEventsGateway
    {
        private readonly IEventsClient _eventsClient;

        /// <summary>
        /// Инициализирует новый экземпляр адаптера Events API.
        /// </summary>
        /// <param name="eventsClient">NSwag-клиент Events API.</param>
        public EventsGateway(IEventsClient eventsClient)
        {
            _eventsClient = eventsClient;
        }

        /// <inheritdoc/>
        public async Task EnsureEventExistsAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                await _eventsClient.EventsGetAsync(eventId, cancellationToken);
            }
            catch (ApiException exception)
            {
                throw new EventsGatewayException(exception.Message, exception.StatusCode, exception);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ReserveSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _eventsClient.ReserveSeatsAsync(eventId, count, cancellationToken);
            }
            catch (ApiException exception)
            {
                throw new EventsGatewayException(exception.Message, exception.StatusCode, exception);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> EventExistsAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _eventsClient.ExistsAsync(eventId, cancellationToken);
            }
            catch (ApiException exception)
            {
                throw new EventsGatewayException(exception.Message, exception.StatusCode, exception);
            }
        }

        /// <inheritdoc/>
        public async Task ReleaseSeatsAsync(Guid eventId, int count, CancellationToken cancellationToken = default)
        {
            try
            {
                await _eventsClient.ReleaseSeatsAsync(eventId, count, cancellationToken);
            }
            catch (ApiException exception)
            {
                throw new EventsGatewayException(exception.Message, exception.StatusCode, exception);
            }
        }
    }
}
