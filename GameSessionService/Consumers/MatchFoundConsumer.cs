using GameSessionService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging.RabbitMQ.Connection;
using System.Text;
using System.Text.Json;

namespace GameSessionService.Consumers
{
    public class MatchFoundConsumer : BackgroundService
    {
        private readonly RabbitMqConnection _rabbitMqConnection;
        private readonly IServiceScopeFactory _scope;

        public MatchFoundConsumer(RabbitMqConnection rabbitMqConnection, IServiceScopeFactory scope)
        {
            this._rabbitMqConnection = rabbitMqConnection;
            this._scope = scope;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("MatchFoundConsumer Started");

            var connection = await _rabbitMqConnection.GetConnectionAsync();

            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
            queue: "match_found",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

            Console.WriteLine("Connected to RabbitMQ. Waiting for messages...");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var body = args.Body.ToArray();

                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"Received message: {message}");

                    var matchFoundEvent = JsonSerializer.Deserialize<MatchFoundEvent>(message);

                    if (matchFoundEvent == null)
                    {
                        Console.WriteLine("Failed to deserialize MatchFoundEvent.");
                        return;
                    }

                    using (var scope = _scope.CreateScope())
                    {
                        var gameSessionService = scope.ServiceProvider.GetRequiredService<IGameService>();
                        await gameSessionService.CreateGameAsync(matchFoundEvent);
                        Console.WriteLine("GameSession created successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error message: {ex.Message}");
                    Console.WriteLine("Error stack trace: " + ex.StackTrace);
                }
                
            };

            await channel.BasicConsumeAsync(
                queue: "match_found",
                autoAck: true,
                consumer: consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        
    }
}
