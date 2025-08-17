namespace RealTimeMongoDashboard.Infrastructure.Config;

public sealed class MongoOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    /// Comma-separated list of allowed collection names (security gate)
    public string AllowedCollections { get; set; } = string.Empty;

    public HashSet<string> GetAllowed() =>
        AllowedCollections.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .ToHashSet(StringComparer.OrdinalIgnoreCase);
}