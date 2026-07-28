using RabbitMQ.Client;
using Shared.Infrastructure.Messaging.RabbitMQ.Connection;
using Shared.Infrastructure.Messaging.RabbitMQ.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Messaging.RabbitMQ.Publishers
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly RabbitMqConnection _connection;

        public RabbitMqPublisher(RabbitMqConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            var connection = await _connection.GetConnectionAsync();

            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var prop = new BasicProperties();

            await channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false , basicProperties: prop, body: body);
        }
    }
}
