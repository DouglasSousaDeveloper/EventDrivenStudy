using Billing.Worker.Consumers;
using Billing.Worker.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<OrderCreatedConsumer>();
builder.Services.AddHostedService<RabbitMqInitializerHostedService>();

var host = builder.Build();
host.Run();