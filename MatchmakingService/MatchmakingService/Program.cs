using MatchmakingService.Background;
using MatchmakingService.Services;
using Shared.Infrastructure.Messaging.RabbitMQ.Options;
using StackExchange.Redis;
using Shared.Infrastructure.Messaging.RabbitMQ.Publishers;
using Shared.Infrastructure.Messaging.RabbitMQ.Connection;
using Shared.Infrastructure.Messaging.RabbitMQ.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("redis:6379"));

builder.Services.AddSingleton<IMatchmakingQueue, RedisMatchmakingQueue>();
builder.Services.AddHostedService<MatchmakingWorker>();
builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
