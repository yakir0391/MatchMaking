using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Infrastructure.Messaging.RabbitMQ.Options
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";

        public string Host { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
