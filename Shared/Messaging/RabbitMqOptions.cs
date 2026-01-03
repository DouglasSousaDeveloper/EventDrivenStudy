namespace Shared.Messaging;

/// <summary>
/// Configurações de conexão com o RabbitMQ.
/// Normalmente carregadas via Environment Variables.
/// </summary>
public sealed class RabbitMqOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; }
    public string User { get; init; } = default!;
    public string Password { get; init; } = default!;
}
