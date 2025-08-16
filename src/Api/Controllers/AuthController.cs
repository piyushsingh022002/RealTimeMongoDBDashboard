using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealTimeMongoDashboard.API.Config;

namespace RealTimeMongoDashboard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly JwtOptions _opts;
    private readonly SymmetricSecurityKey _key;

    public AuthController(IOptions<JwtOptions> opts)
    {
        _opts = opts.Value;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
    }

    // Demo login; replace with real user validation as needed
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public IActionResult IssueToken([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return Unauthorized();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, req.Username),
            new(ClaimTypes.Name, req.Username),
            new(ClaimTypes.Role, "user")
        };

        return Ok(new TokenResponse { access_token = CreateJwt(claims) });
    }

    // For Watcher microservice
    [HttpPost("service-token")]
    public IActionResult IssueServiceToken([FromHeader(Name = "x-internal-key")] string keyHeader)
    {
        if (!string.Equals(keyHeader, _opts.Internal.ApiKey, StringComparison.Ordinal))
            return Unauthorized();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "watcher"),
            new(ClaimTypes.Role, "internal")
        };
        return Ok(new TokenResponse { access_token = CreateJwt(claims) });
    }

    private string CreateJwt(IEnumerable<Claim> claims)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_opts.ExpiryMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public sealed class LoginRequest
    {
        public string Username { get; init; } = default!;
        public string Password { get; init; } = default!;
    }
    public sealed class TokenResponse { public string access_token { get; init; } = default!; }
}
