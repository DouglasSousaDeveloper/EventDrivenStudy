using RabbitMQ.Client;
using Shared.RabbitMq;

namespace Notification.Worker.Infrastructure;

public sealed class RabbitMqInitializerHostedService : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitMqInitializerHostedService> _logger;

    public RabbitMqInitializerHostedService(
        IConnectionFactory connectionFactory,
        ILogger<RabbitMqInitializerHostedService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initializing RabbitMQ topology for Notification.Worker");

        await using var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync();

        // Exchange principal (compartilhado)
        await channel.ExchangeDeclareAsync(
            RabbitMqTopology.OrderExchange,
            ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        // Exchange de retry (Notification)
        await channel.ExchangeDeclareAsync(
            RabbitMqTopology.NotificationRetryExchange,
            ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        // Fila principal
        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.NotificationQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = RabbitMqTopology.NotificationRetryExchange,
                ["x-dead-letter-routing-key"] = RabbitMqTopology.OrderCreatedRoutingKey
            },
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            RabbitMqTopology.NotificationQueue,
            RabbitMqTopology.OrderExchange,
            RabbitMqTopology.OrderCreatedRoutingKey,
            cancellationToken: stoppingToken);

        // Fila de retry (TTL)
        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.NotificationRetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-message-ttl"] = RabbitMqTopology.RetryDelayMilliseconds,
                ["x-dead-letter-exchange"] = RabbitMqTopology.OrderExchange,
                ["x-dead-letter-routing-key"] = RabbitMqTopology.OrderCreatedRoutingKey
            },
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            RabbitMqTopology.NotificationRetryQueue,
            RabbitMqTopology.NotificationRetryExchange,
            RabbitMqTopology.OrderCreatedRoutingKey,
            cancellationToken: stoppingToken);

        // DLQ
        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.NotificationDlqQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMQ topology for Notification.Worker initialized successfully");
    }
}