using GameSessionService.Data;
using GameSessionService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using System.Collections.Concurrent;

namespace GameSessionService.Services
{
    public class GameService : IGameService
    {
        private readonly GameSessionDbContext _db;

        public GameService(GameSessionDbContext db)
        {
            _db = db;
        }
        public async Task<GameSession> CreateGameAsync(MatchFoundEvent matchFoundEvent)
        {
            var gameSession = new GameSession
            {
                Id = Guid.NewGuid(),
                Player1Id = matchFoundEvent.Player1Id,
                Player2Id = matchFoundEvent.Player2Id,
                CreatedAt = matchFoundEvent.CreatedAt,
                Status = "Created"
            };

            _db.GameSessions.Add(gameSession);

            await _db.SaveChangesAsync();

            Console.WriteLine($"Game created: {gameSession.Id} for players {gameSession.Player1Id} and {gameSession.Player2Id}");

            return gameSession;
        }

        public async Task<List<GameSession>> GetAllAsync()
        {
            return await _db.GameSessions.ToListAsync();
        }
    }
}
