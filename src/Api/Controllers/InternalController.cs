using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeMongoDashboard.Application.Interfaces;
using RealTimeMongoDashboard.Domain.Models;

namespace RealTimeMongoDashboard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InternalController : ControllerBase
{
    private readonly INotifier _notifier;
    public InternalController(INotifier notifier) => _notifier = notifier;

    // Watcher + manual test
    [HttpPost("broadcast")]
    [Authorize(Roles = "internal")]
    public async Task<IActionResult> Broadcast([FromBody] ChangeMessage message, CancellationToken ct)
    {
        await _notifier.BroadcastAsync(message, ct);
        return Accepted();
    }

    // Quick manual test from Swagger: requires internal token
    [HttpPost("ping")]
    [Authorize(Roles = "internal")]
    public IActionResult Ping() => Ok(new { ok = true, ts = DateTime.UtcNow });
}
