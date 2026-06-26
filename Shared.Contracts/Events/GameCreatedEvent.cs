using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Events
{
    public class GameCreatedEvent
    {
        public Guid GameId { get; set; }

        public string Player1Id { get; set; } = default!;
        public string Player2Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }
}
