using Shared.Events;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Shared.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher(RabbitMqOptions options)
    {
        var resultConnection = GetConnectionAndChannel(options).Result;
        _connection = resultConnection.Item1;
        _channel = resultConnection.Item2;
    }

    public async Task<(IConnection, IChannel)> GetConnectionAndChannel(RabbitMqOptions options)
    {
        var factory = new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.User,
            Password = options.Password,
            VirtualHost = "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        return (connection, channel);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string exchange, string routingKey,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        // Declara exchange (idempotente)
        await _channel.ExchangeDeclareAsync(
            exchange: exchange,
            type: ExchangeType.Topic,
            durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event);

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = @event.EventId.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
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

    public void Dispose()
    {
        _connection.CloseAsync();
        _channel.CloseAsync();
    }
}
