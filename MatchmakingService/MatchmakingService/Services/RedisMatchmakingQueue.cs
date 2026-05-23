using MatchmakingService.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace MatchmakingService.Services
{
    public class RedisMatchmakingQueue : IMatchmakingQueue
    {
        private readonly IDatabase _redis;

        private const string QueueListKey = "matchmaking_queue_list";
        private const string QueueSetKey = "matchmaking_queue_set";

        public RedisMatchmakingQueue(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public bool Enqueue(PlayerQueueEntry player)
        {
            if(_redis.SetContains(QueueSetKey, player.PlayerId))
            {
                return false; // Player is already in the queue
            }

            var json = JsonSerializer.Serialize(player);
            var transaction = _redis.CreateTransaction();

            transaction.ListRightPushAsync(QueueListKey, json);
            transaction.SetAddAsync(QueueSetKey, player.PlayerId);

            return transaction.Execute();
        }

        public List<PlayerQueueEntry> GetAll()
        {
            return _redis.ListRange(QueueListKey)
                         .Select(player => JsonSerializer.Deserialize<PlayerQueueEntry>(player!)!)
                         .ToList();
        }

        public bool Remove(string playerId)
        {
            if(!_redis.SetContains(QueueSetKey, playerId))
            {
                return false; // Player is not in the queue
            }

            var players = _redis.ListRange(QueueListKey);

            foreach(var player in players)
            {
                var entry = JsonSerializer.Deserialize<PlayerQueueEntry>(player!);
                if(entry?.PlayerId == playerId)
                {
                    var transaction = _redis.CreateTransaction();
                    transaction.ListRemoveAsync(QueueListKey, player);
                    transaction.SetRemoveAsync(QueueSetKey, playerId);
                    return transaction.Execute();
                }
            }
            return false;
        }
    }
}
