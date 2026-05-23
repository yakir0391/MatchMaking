using MatchmakingService.Models;
using System.Collections.Concurrent;

namespace MatchmakingService.Services
{
    public class MatchmakingQueue : IMatchmakingQueue
    {
        private readonly ConcurrentDictionary<string, PlayerQueueEntry> _players = new();

        public bool Enqueue(PlayerQueueEntry player)
        {
            return _players.TryAdd(player.PlayerId, player);
        }

        public bool Remove(string playerId)
        {
            return _players.TryRemove(playerId, out _);
        }

        public List<PlayerQueueEntry> GetAll()
        {
            return _players.Values.ToList();
        }
    }
}
