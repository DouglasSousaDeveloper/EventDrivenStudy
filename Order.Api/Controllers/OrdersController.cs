using Microsoft.AspNetCore.Mvc;
using Order.Api.Models;
using Shared.Events;
using Shared.Messaging;

namespace Order.API.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IRabbitMqPublisher _publisher;

    public OrdersController(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var orderId = Guid.NewGuid();

        var orderCreatedEvent = new OrderCreatedEvent(
            orderId,
            request.CustomerId,
            request.Amount,
            DateTime.UtcNow);

        await _publisher.PublishAsync(
            orderCreatedEvent,
            routingKey: "order.created");

        return Accepted(new
        {
            OrderId = orderId,
            Status = "OrderCreatedEvent published"
        });
    }
}
