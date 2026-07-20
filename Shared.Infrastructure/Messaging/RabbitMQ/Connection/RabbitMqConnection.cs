using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Shared.Infrastructure.Messaging.RabbitMQ.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Messaging.RabbitMQ.Connection
{
    public class RabbitMqConnection
    {
        private readonly RabbitMqOptions _options;
        private IConnection? _connection;

        public RabbitMqConnection(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            if (_connection != null && _connection.IsOpen)
                return _connection;

            const int maxRetries = 5;

            for (int retry = 1; retry <= maxRetries; retry++)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _options.Host,
                        UserName = _options.User,
                        Password = _options.Password
                    };

                    _connection = await factory.CreateConnectionAsync();

                    Console.WriteLine("RabbitMQ connection established.");

                    return _connection;
                }
                catch (BrokerUnreachableException ex)
                {
                    Console.WriteLine($"RabbitMQ unavailable. Retry {retry}/{maxRetries}");

                    await Task.Delay(3000);
                }
            }

            throw new InvalidOperationException(
                "Unable to connect to RabbitMQ.");
        }
    }
}
