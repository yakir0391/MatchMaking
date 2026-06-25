
using MatchmakingService.Services;
using MatchmakingService.SharedContracts;
using Microsoft.Extensions.DependencyInjection;

namespace MatchmakingService.Background
{
    public class MatchmakingWorker : BackgroundService
    {
        private readonly IMatchmakingQueue _queue;
        private IServiceScopeFactory _serviceScopeFactory;
        public MatchmakingWorker(IMatchmakingQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
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

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var publisher = scope.ServiceProvider.GetRequiredService<RabbitMqPublisher>();

                        var evt = new MatchFoundEvent
                        {
                            Player1Id = player1.PlayerId,
                            Player2Id = player2.PlayerId
                        };

                        await publisher.PublishAsync(evt);

                        Console.WriteLine($"Published Match found event for players {player1.PlayerId} and {player2.PlayerId}");
                    }

                    _queue.Remove(player1.PlayerId);
                    _queue.Remove(player2.PlayerId);
                }

                await Task.Delay(10000, stoppingToken);
            } 
        }
    }
}
