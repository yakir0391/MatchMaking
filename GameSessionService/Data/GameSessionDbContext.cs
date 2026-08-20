using GameSessionService.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSessionService.Data
{
    public class GameSessionDbContext : DbContext
    {
        public GameSessionDbContext(DbContextOptions<GameSessionDbContext> options) : base(options)
        {
        }
        public DbSet<GameSession> GameSessions => Set<GameSession>();
    }
}
