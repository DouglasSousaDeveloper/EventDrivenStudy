using Shared.Events;

namespace Billing.Worker.Consumers;

public sealed class OrderCreatedConsumer
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(
        OrderCreatedEvent @event,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing billing for OrderId {OrderId} | Amount {Amount}",
            @event.OrderId,
            @event.Amount);

        // Simula processamento de billing
        await Task.Delay(500, cancellationToken);

        _logger.LogInformation(
            "Billing processed successfully for OrderId {OrderId}",
            @event.OrderId);
    }
}
