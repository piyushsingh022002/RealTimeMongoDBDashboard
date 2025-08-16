using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InternalController : ControllerBase
{
    private readonly INotifier _notifier;
    private readonly string _internalKey;

    public InternalController(INotifier notifier, IConfiguration cfg)
    {
        _notifier = notifier;
        _internalKey = Environment.GetEnvironmentVariable("INTERNAL_KEY")
                      ?? cfg["INTERNAL_KEY"]
                      ?? string.Empty;
    }

    public sealed record BroadcastRequest([
        Required] string Collection,
        [Required] string OperationType,
        string? Id,
        object? Document,
        DateTime? TimestampUtc
    );

    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] BroadcastRequest req)
    {
        var headerKey = Request.Headers["X-Internal-Key"].ToString();
        if (string.IsNullOrEmpty(_internalKey) || headerKey != _internalKey)
            return Unauthorized();

        var msg = new ChangeMessage
        {
            Collection = req.Collection,
            OperationType = req.OperationType,
            Id = req.Id,
            Document = req.Document,
            TimestampUtc = req.TimestampUtc ?? DateTime.UtcNow
        };

        await _notifier.BroadcastAsync("DataChanged", msg, HttpContext.RequestAborted);
        return Accepted();
    }
}