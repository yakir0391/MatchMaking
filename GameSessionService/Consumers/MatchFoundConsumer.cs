using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Infrastructure.Messaging.RabbitMQ.Connection;

namespace GameSessionService.Consumers
{
    public class MatchFoundConsumer : BackgroundService
    {
        private readonly RabbitMqConnection _rabbitMqConnection;

        public MatchFoundConsumer(RabbitMqConnection rabbitMqConnection)
        {
            this._rabbitMqConnection = rabbitMqConnection;
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
                var body = args.Body.ToArray();

                var message = System.Text.Encoding.UTF8.GetString(body);

                Console.WriteLine($"Received message: {message}");

                await Task.CompletedTask;
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
