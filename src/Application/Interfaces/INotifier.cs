namespace Application.Interfaces;

public interface INotifier
{
    Task NotifyAllAsync(string message);
}