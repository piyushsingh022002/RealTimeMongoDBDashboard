namespace RealTimeMongoDashboard.Infrastructure.Config;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public string Key { get; init; } = default!;
    public int ExpiryMinutes { get; init; } = 60;
    public InternalOptions Internal { get; init; } = new();
    public sealed class InternalOptions { public string ApiKey { get; init; } = default!; }
}
