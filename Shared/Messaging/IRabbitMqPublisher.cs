using Shared.Events;

namespace Shared.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        string exchange,
        string routingKey,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
