using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using API.Hubs;

namespace API.Services
{
    public class Notifier : INotifier
    {
        private readonly IHubContext<NotificationsHub> _hubContext;

        public Notifier(IHubContext<NotificationsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAllAsync(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
