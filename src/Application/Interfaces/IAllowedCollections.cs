namespace RealTimeMongoDashboard.Application.Interfaces;

public interface IAllowedCollections
{
    bool IsAllowed(string collection);
}
