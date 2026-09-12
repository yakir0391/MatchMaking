using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var playerId = Context.GetHttpContext()?.Request.Query["playerId"].ToString();

            if (!string.IsNullOrEmpty(playerId))
            {
                Console.WriteLine($"Player {playerId} connected to NotificationHub");

                Context.Items["PlayerId"] = playerId;
            }
            else
            {
                Console.WriteLine("Player ID not provided in query string.");
            }


            await base.OnConnectedAsync();
        }
    }
}
