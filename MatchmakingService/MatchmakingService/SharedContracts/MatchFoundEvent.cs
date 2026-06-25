namespace MatchmakingService.SharedContracts
{
    public class MatchFoundEvent
    {
        public string Player1Id { get; set; } = default!;
        public string Player2Id { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
