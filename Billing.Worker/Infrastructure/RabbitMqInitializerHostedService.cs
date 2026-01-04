using RabbitMQ.Client;
using Shared.RabbitMq;

namespace Billing.Worker.Infrastructure;

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
        _logger.LogInformation("Initializing RabbitMQ topology for Billing.Worker");

        await using var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync();

        // Exchange principal (eventos de domínio)
        await channel.ExchangeDeclareAsync(
            RabbitMqTopology.OrderExchange,
            ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        // Exchange de retry
        await channel.ExchangeDeclareAsync(
            RabbitMqTopology.BillingRetryExchange,
            ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        // =========================
        // FILA PRINCIPAL
        // =========================
        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.BillingQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                // Se falhar → vai para retry
                ["x-dead-letter-exchange"] = RabbitMqTopology.BillingRetryExchange,
                ["x-dead-letter-routing-key"] = RabbitMqTopology.OrderCreatedRoutingKey
            },
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            RabbitMqTopology.BillingQueue,
            RabbitMqTopology.OrderExchange,
            RabbitMqTopology.OrderCreatedRoutingKey,
            cancellationToken: stoppingToken);

        // =========================
        // FILA DE RETRY (TTL)
        // =========================
        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.BillingRetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                // Tempo de espera antes de tentar novamente
                ["x-message-ttl"] = RabbitMqTopology.RetryDelayMilliseconds,

                // Após TTL → volta para exchange principal
                ["x-dead-letter-exchange"] = RabbitMqTopology.OrderExchange,
                ["x-dead-letter-routing-key"] = RabbitMqTopology.OrderCreatedRoutingKey
            },
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            RabbitMqTopology.BillingRetryQueue,
            RabbitMqTopology.BillingRetryExchange,
            RabbitMqTopology.OrderCreatedRoutingKey,
            cancellationToken: stoppingToken);

        // =========================
        // DEAD LETTER QUEUE (DLQ)
        // =========================
        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.BillingDlqQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Binding da DLQ
        await channel.QueueBindAsync(
            RabbitMqTopology.BillingDlqQueue,
            RabbitMqTopology.OrderExchange,
            RabbitMqTopology.OrderCreatedDlqRoutingKey,
            cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMQ topology initialized successfully");
    }
}
