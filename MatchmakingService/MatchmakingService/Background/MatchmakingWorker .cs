
using MatchmakingService.Services;

namespace MatchmakingService.Background
{
    public class MatchmakingWorker : BackgroundService
    {
        private readonly IMatchmakingQueue _queue;
        public MatchmakingWorker(IMatchmakingQueue queue)
        {
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Running matchmaking worker...");

                var players = _queue.GetAll().OrderBy(p => p.JoinedAt).ToList();

                for (int i = 0; i < players.Count - 1; i += 2)
                {
                    var player1 = players[i];
                    var player2 = players[i + 1];

                    Console.WriteLine($"Matched {player1.PlayerId} with {player2.PlayerId}");

                    _queue.Remove(player1.PlayerId);
                    _queue.Remove(player2.PlayerId);
                }

                await Task.Delay(10000, stoppingToken);
            } 
        }
    }
}
