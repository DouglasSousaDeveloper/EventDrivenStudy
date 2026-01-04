using Notification.Worker.Consumers;
using Notification.Worker.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<OrderCreatedNotificationConsumer>();
builder.Services.AddHostedService<RabbitMqInitializerHostedService>();

var host = builder.Build();
host.Run();