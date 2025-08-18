using RealTimeMongoDashboard.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using RealTimeMongoDashboard.API.Hubs;
using RealTimeMongoDashboard.Domain.Models;

namespace RealTimeMongoDashboard.API.Services
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

        public async Task BroadcastAsync(ChangeMessage message, CancellationToken ct = default)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message, ct);
        }
    
    }
}
