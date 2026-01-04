using Billing.Worker.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using Shared.RabbitMq;
using Shared.Serialization;
using System.Text;

namespace Billing.Worker.Infrastructure.RabbitMq;

public sealed class BillingConsumerHostedService : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly OrderCreatedConsumer _consumer;
    private readonly ILogger<BillingConsumerHostedService> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public BillingConsumerHostedService(
        IConnectionFactory connectionFactory,
        OrderCreatedConsumer consumer,
        ILogger<BillingConsumerHostedService> logger)
    {
        _connectionFactory = connectionFactory;
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Billing RabbitMQ consumer");

        _connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: RabbitMqTopology.BillingQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var orderCreated = json.Deserialize<OrderCreatedEvent>();

            await _consumer.HandleAsync(orderCreated, args.CancellationToken);

            await _channel!.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing billing message");

            var retryCount = GetRetryCount(args.BasicProperties.Headers);

            if (retryCount >= RabbitMqTopology.MaxRetryCount)
            {
                await _channel!.BasicRejectAsync(args.DeliveryTag, requeue: false);
                return;
            }

            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false);
        }
    }

    private static int GetRetryCount(IDictionary<string, object?>? headers)
    {
        if (headers is null)
            return 0;

        if (!headers.TryGetValue("x-death", out var value))
            return 0;

        if (value is not IList<object> deaths || deaths.Count == 0)
            return 0;

        if (deaths[0] is not IDictionary<string, object?> death)
            return 0;

        if (!death.TryGetValue("count", out var count))
            return 0;

        return Convert.ToInt32(count);
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection is not null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
