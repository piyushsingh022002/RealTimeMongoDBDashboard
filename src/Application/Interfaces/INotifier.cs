namespace Application.Interfaces;

public interface INotifier
{
    Task BroadcastAsync(string method, object payload, CancellationToken ct = default);
}