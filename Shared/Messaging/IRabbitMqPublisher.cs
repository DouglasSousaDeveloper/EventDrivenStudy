using Shared.Events;

namespace Shared.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        string routingKey,
        CancellationToken cancellationToken = default);
}
