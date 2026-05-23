using MatchmakingService.Models;

namespace MatchmakingService.Services
{
    public interface IMatchmakingQueue
    {
        bool Enqueue(PlayerQueueEntry player);
        bool Remove(string playerId);
        List<PlayerQueueEntry> GetAll();
    }
}
