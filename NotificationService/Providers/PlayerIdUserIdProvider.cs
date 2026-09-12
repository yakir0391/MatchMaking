using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Providers
{
    public class PlayerIdUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.GetHttpContext()?.Request.Query["playerId"].ToString();
        }
    }
}
