namespace Infrastructure.Config;

public sealed class JwtOptions
{
    public string Key { get; set; } = string.Empty; // symmetric key
    public string Issuer { get; set; } = "rtmdash";
    public string Audience { get; set; } = "rtmdash.clients";
    public int ExpiryMinutes { get; set; } = 720; // 12h default
}