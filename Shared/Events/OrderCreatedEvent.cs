namespace Shared.Events;

/// <summary>
/// Evento publicado quando um pedido é criado com sucesso.
/// </summary>
public sealed class OrderCreatedEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }

    public decimal TotalAmount { get; init; }

    public string Currency { get; init; } = default!;

    public DateTime CreatedAtUtc { get; init; }
}
