using Shared.Messaging;

namespace Order.API.Infrastructure;

public sealed class RabbitMqInitializerHostedService : BackgroundService
{
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<RabbitMqInitializerHostedService> _logger;

    public RabbitMqInitializerHostedService(
        RabbitMqPublisher publisher,
        ILogger<RabbitMqInitializerHostedService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initializing RabbitMQ connection...");

        await _publisher.InitializeAsync(stoppingToken);

        _logger.LogInformation("RabbitMQ initialized successfully.");

        // Mantém o serviço vivo enquanto a aplicação estiver rodando
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Esperado no shutdown
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down RabbitMQ connection...");

        await _publisher.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
