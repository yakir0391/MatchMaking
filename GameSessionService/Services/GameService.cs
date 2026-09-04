using GameSessionService.Data;
using GameSessionService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging.RabbitMQ.Publishers;
using System.Collections.Concurrent;

namespace GameSessionService.Services
{
    public class GameService : IGameService
    {
        private readonly GameSessionDbContext _db;
        private readonly RabbitMqPublisher _publisher;

        public GameService(GameSessionDbContext db, RabbitMqPublisher publisher)
        {
            _db = db;
            _publisher = publisher;
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

            var gameCreatedEvent = new GameCreatedEvent
            {
                GameId = gameSession.Id,
                Player1Id = gameSession.Player1Id,
                Player2Id = gameSession.Player2Id,
                CreatedAt = gameSession.CreatedAt,
                Status = gameSession.Status
            };

            await _publisher.PublishAsync("game_created", gameCreatedEvent);

            Console.WriteLine($"Published GameCreatedEvent for game {gameSession.Id}");

            return gameSession;
        }

        public async Task<List<GameSession>> GetAllAsync()
        {
            return await _db.GameSessions.ToListAsync();
        }

        public async Task<GameSession> GetByIdAsync(Guid id)
        {
            return await _db.GameSessions.FirstOrDefaultAsync(game => game.Id == id);
        }
    }
}
