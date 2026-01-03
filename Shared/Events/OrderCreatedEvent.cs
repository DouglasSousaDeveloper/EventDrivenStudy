namespace Shared.Events;

/// <summary>
/// Evento publicado quando um pedido é criado com sucesso.
/// </summary>
public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    DateTime CreatedAt);