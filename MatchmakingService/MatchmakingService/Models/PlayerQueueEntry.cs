namespace MatchmakingService.Models
{
    public class PlayerQueueEntry
    {
        public string PlayerId { get; set; } 
        public int Rank { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
