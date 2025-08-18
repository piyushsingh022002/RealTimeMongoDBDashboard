using Microsoft.AspNetCore.SignalR;

namespace RealTimeMongoDashboard.API.Hubs
{
    public class NotificationsHub : Hub
    {

         public override async Task OnConnectedAsync() //optional just to check if the user is connected to the hubs
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "Connected to NotificationsHub");
            await base.OnConnectedAsync();
        }
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
