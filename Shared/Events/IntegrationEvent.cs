namespace Shared.Events;

/// <summary>
/// Evento base para integração entre serviços.
/// Todos os eventos publicados no broker devem herdar dessa classe.
/// </summary>
public abstract class IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
}
