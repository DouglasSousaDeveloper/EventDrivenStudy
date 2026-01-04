using Shared.Events;

namespace Notification.Worker.Consumers;

public sealed class OrderCreatedNotificationConsumer
{
    private readonly ILogger<OrderCreatedNotificationConsumer> _logger;

    public OrderCreatedNotificationConsumer(ILogger<OrderCreatedNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(
        OrderCreatedEvent @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending notification for OrderId {OrderId} | Customer {CustomerId}",
            @event.OrderId,
            @event.CustomerId);

        // Simula envio de notificação (email, push, etc.)
        await Task.Delay(300, cancellationToken);

        _logger.LogInformation(
            "Notification sent successfully for OrderId {OrderId}",
            @event.OrderId);
    }
}
