using GameSessionService.Models;
using Shared.Contracts.Events;

namespace GameSessionService.Services
{
    public interface IGameService
    {
        Task<GameSession> CreateGameAsync(MatchFoundEvent matchFoundEvent);
        Task<List<GameSession>> GetAllAsync();
    }
}
