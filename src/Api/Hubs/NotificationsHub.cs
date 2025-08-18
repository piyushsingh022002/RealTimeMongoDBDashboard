using Microsoft.AspNetCore.SignalR;

namespace RealTimeMongoDashboard.API.Hubs
{
    public class NotificationsHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
