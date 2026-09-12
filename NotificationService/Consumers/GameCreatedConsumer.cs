using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging.RabbitMQ.Connection;
using System.Text;
using System.Text.Json;

namespace NotificationService.Consumers
{
    public class GameCreatedConsumer : BackgroundService
    {
        private readonly RabbitMqConnection _rabbitMqConnection;
        private readonly IHubContext<NotificationHub> _hubContext;

        public GameCreatedConsumer(RabbitMqConnection rabbitMqConnection, IHubContext<NotificationHub> hubContext)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("GameCreatedConsumer Started");

            var connection = await _rabbitMqConnection.GetConnectionAsync();

            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "game_created",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            Console.WriteLine("Connected to RabbitMQ. Waiting for game_created messages...");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();

                    var json = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"Received GameCreated event: {json}");

                    var gameCreatedEvent = JsonSerializer.Deserialize<GameCreatedEvent>(json);

                    if (gameCreatedEvent == null)
                    {
                        Console.WriteLine("Failed to deserialize GameCreatedEvent.");

                        return;
                    }

                    Console.WriteLine($"Game created: {gameCreatedEvent.GameId}");

                    Console.WriteLine($"Players: {gameCreatedEvent.Player1Id} vs {gameCreatedEvent.Player2Id}");

                    await _hubContext.Clients.Users(gameCreatedEvent.Player1Id, gameCreatedEvent.Player2Id)
                        .SendAsync("GameCreated", gameCreatedEvent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error processing GameCreated event: {ex.Message}");
                }
            };

            await channel.BasicConsumeAsync(
                queue: "game_created",
                autoAck: true,
                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
