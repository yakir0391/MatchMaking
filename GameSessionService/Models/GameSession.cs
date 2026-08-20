namespace GameSessionService.Models
{
    public class GameSession
    {
        public Guid Id { get; set; }

        public string Player1Id { get; set; } = default!;

        public string Player2Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = "Created";
    }
}
