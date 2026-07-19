using GameSessionService.Models;
using System.Collections.Concurrent;

namespace GameSessionService.Services
{
    public class GameService
    {
        private readonly ConcurrentDictionary<Guid, GameSession> _games = new();

        public void Add(GameSession gameSession)
        {
            _games.TryAdd(gameSession.Id, gameSession);
        }

        public IEnumerable<GameSession> GetAll()
        {
            return _games.Values;
        }
    }
}
