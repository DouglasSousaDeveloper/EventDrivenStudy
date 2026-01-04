using Billing.Worker.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using System.Text;
using System.Text.Json;

namespace Billing.Worker.Infrastructure;

public sealed class RabbitMqInitializerHostedService : BackgroundService
{
    private readonly ILogger<RabbitMqInitializerHostedService> _logger;
    private readonly OrderCreatedConsumer _consumer;
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqInitializerHostedService(
        ILogger<RabbitMqInitializerHostedService> logger,
        OrderCreatedConsumer consumer,
        IConfiguration configuration)
    {
        _logger = logger;
        _consumer = consumer;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"],
            Port = int.Parse(_configuration["RabbitMq:Port"]!),
            UserName = _configuration["RabbitMq:Username"],
            Password = _configuration["RabbitMq:Password"],
            VirtualHost = "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();

        var exchange = _configuration["RabbitMq:Exchange"]!;
        var queue = _configuration["RabbitMq:Queue"]!;
        var routingKey = _configuration["RabbitMq:RoutingKey"]!;

        await _channel.ExchangeDeclareAsync(
            exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue,
            exchange,
            routingKey,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                await _consumer.HandleAsync(@event, stoppingToken);

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (JsonException ex)
            {
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OrderCreatedEvent");
                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    false,
                    requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue,
            autoAck: false,
            consumer);

        _logger.LogInformation("Billing.Worker is consuming messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Billing.Worker...");

        if (_channel is not null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection is not null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
