using System.Text.Json;
using RabbitMQ.Client;
using Shared.Events;

namespace Shared.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(RabbitMqOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Inicializa conexão e channel de forma assíncrona.
    /// Deve ser chamado na inicialização da aplicação.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.User,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync();
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        string exchange,
        string routingKey,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (_channel is null)
            throw new InvalidOperationException(
                "RabbitMqPublisher não inicializado. Chame InitializeAsync().");

        // Declara exchange (idempotente)
        await _channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event);
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}