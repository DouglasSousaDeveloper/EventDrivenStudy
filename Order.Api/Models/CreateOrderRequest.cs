namespace Order.Api.Models;

public class CreateOrderRequest
{
    public Guid CustomerId { get; init; }
    public decimal Amount { get; init; }
}
