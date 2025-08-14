using Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Api.Hubs; // careful: referenced from API assembly name

namespace Infrastructure.Services;

public sealed class Notifier(IHubContext<NotificationsHub> hub) : INotifier
{
    private readonly IHubContext<NotificationsHub> _hub = hub;

    public Task BroadcastAsync(string method, object payload, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync(method, payload, ct);
}