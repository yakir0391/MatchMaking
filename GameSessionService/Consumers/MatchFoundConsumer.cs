
namespace GameSessionService.Consumers
{
    public class MatchFoundConsumer : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("MatchFoundConsumer Started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);

                Console.WriteLine("Waiting for messages...");
            }
        }
    }
}
