using MatchmakingService.Models;
using MatchmakingService.Services;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Events;
using Shared.Infrastructure.Messaging.RabbitMQ.Interfaces;

namespace MatchmakingService.Background
{
    public class MatchmakingWorker : BackgroundService
    {
        private readonly IMatchmakingQueue _queue;
        private readonly IRabbitMqPublisher _publisher;

        private const int BaseRankDifference = 100;
        private const int RankExpansionPer30Seconds = 100;

        public MatchmakingWorker(IMatchmakingQueue queue, IRabbitMqPublisher publisher)
        {
            _queue = queue;
            _publisher = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Running matchmaking worker...");

                var players = _queue.GetAll().OrderBy(p => p.JoinedAt).ToList();

                var matchedPlayerIds = new HashSet<string>();

                foreach (var player1 in players) 
                {
                    if (matchedPlayerIds.Contains(player1.PlayerId)) 
                    {
                        continue; 
                    } 

                    var player2 = FindBestMatch(player1, players, matchedPlayerIds); 
                    
                    if (player2 == null) 
                    {
                        continue; 
                    }

                    var evt = new MatchFoundEvent { Player1Id = player1.PlayerId, Player2Id = player2.PlayerId, CreatedAt = DateTime.UtcNow };
                    
                    await _publisher.PublishAsync("match_found", evt);
                    
                    Console.WriteLine($"Published Match found event for players " + $"{player1.PlayerId} and {player2.PlayerId}");
                    
                    _queue.Remove(player1.PlayerId); 
                    _queue.Remove(player2.PlayerId); 
                    
                    matchedPlayerIds.Add(player1.PlayerId);
                    matchedPlayerIds.Add(player2.PlayerId); 
                }

                await Task.Delay(10000, stoppingToken);
            } 
        }

        private PlayerQueueEntry? FindBestMatch(PlayerQueueEntry player1, List<PlayerQueueEntry> players, HashSet<string> matchedPlayerIds) 
        {
            var waitingTime = DateTime.UtcNow - player1.JoinedAt;
            
            var expansionSteps = (int)(waitingTime.TotalSeconds / 30); 
            
            var allowedRankDifference = BaseRankDifference + (expansionSteps * RankExpansionPer30Seconds);
            
            var candidates = players.Where(player => player.PlayerId != player1.PlayerId && !matchedPlayerIds.Contains(player.PlayerId))
                .Where(player => Math.Abs(player.Rank - player1.Rank) <= allowedRankDifference)
                .OrderBy(player => Math.Abs(player.Rank - player1.Rank))
                .ThenBy(player => player.JoinedAt)
                .ToList(); 
            
            return candidates.FirstOrDefault(); }
    }
}
