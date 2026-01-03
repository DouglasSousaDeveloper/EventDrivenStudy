using Order.API.Infrastructure;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// RabbitMQ
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<IRabbitMqPublisher>(
    sp => sp.GetRequiredService<RabbitMqPublisher>());

builder.Services.AddHostedService<RabbitMqInitializerHostedService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
