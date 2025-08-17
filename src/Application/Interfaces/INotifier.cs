using RealTimeMongoDashboard.Domain.Models;
namespace RealTimeMongoDashboard.Application.Interfaces;

public interface INotifier
{
    Task NotifyAllAsync(string message);
    Task BroadcastAsync(ChangeMessage message, CancellationToken ct = default);
}