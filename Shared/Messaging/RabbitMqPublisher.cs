using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Shared.Serialization;

namespace Shared.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;
    private string _exchange = default!;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"],
            Port = int.Parse(_configuration["RabbitMq:Port"]!),
            UserName = _configuration["RabbitMq:Username"],
            Password = _configuration["RabbitMq:Password"]
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync();

        _exchange = _configuration["RabbitMq:Exchange"]!;

        await _channel.ExchangeDeclareAsync(
            _exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        string routingKey,
        CancellationToken cancellationToken = default)
    {
        if (_channel is null)
            throw new InvalidOperationException("RabbitMQ not initialized.");

        var body = @event.Serialize();

        await _channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.CloseAsync();

        if (_connection is not null)
            await _connection.CloseAsync();
    }
}
