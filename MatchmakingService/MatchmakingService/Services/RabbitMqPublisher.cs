using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace MatchmakingService.Services
{
    public class RabbitMqPublisher
    {
        private readonly IConnection _connection;
        private IChannel _channel;
        private readonly IConfiguration _config;
        private readonly string _queueName = "match_found";

        public RabbitMqPublisher(IConfiguration config)
        {
            _config = config;
            _connection = GetConnection().Result;
        }

        private async Task<IConnection> GetConnection()
        {
            int retryCount = 0;
            const int maxRetries = 5;
            const int delayMs = 5000;

            while (retryCount < maxRetries)
            {
                try
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = _config["RabbitMQ:Host"],
                        UserName = _config["RabbitMQ:User"],
                        Password = _config["RabbitMQ:Password"],
                    };


                    return await factory.CreateConnectionAsync();
                }
                catch (BrokerUnreachableException ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        return null;
                    }
                    Task.Delay(delayMs).Wait();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }

        public async Task PublishAsync<T>(T message)
        {
            if (_connection == null)
            {
                return;
            }

            try
            {
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(queue: _queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var json = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var prop = new BasicProperties();

                await _channel.BasicPublishAsync(exchange: "",
                                     routingKey: _queueName,
                                     mandatory: false,
                                     basicProperties: prop,
                                     body: json);

                Console.WriteLine($"Published message to RabbitMQ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during RabbitMQ publish : {ex.Message}");
            }
        }
    }
}
