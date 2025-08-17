namespace RealTimeMongoDashboard.Domain.Models;

public sealed class ChangeMessage
{
    public required string Collection { get; init; }
    public required string OperationType { get; init; } // insert|update|replace|delete
    public string? Id { get; init; }
    public object? Document { get; init; }
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}