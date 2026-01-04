using Billing.Worker.Consumers;
using Billing.Worker.Infrastructure;
using Billing.Worker.Infrastructure.RabbitMq;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

// ============================
// RabbitMQ Infrastructure
// ============================
builder.Services.AddSingleton<IConnectionFactory>(_ =>
{
    return new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMq:Host"],
        Port = int.Parse(builder.Configuration["RabbitMq:Port"]!),
        UserName = builder.Configuration["RabbitMq:Username"],
        Password = builder.Configuration["RabbitMq:Password"]
    };
});

// ============================
// Application Consumers
// ============================
builder.Services.AddSingleton<OrderCreatedConsumer>();

// ============================
// Hosted Services
// ============================
builder.Services.AddHostedService<RabbitMqInitializerHostedService>();
builder.Services.AddHostedService<BillingConsumerHostedService>();

var host = builder.Build();
host.Run();
