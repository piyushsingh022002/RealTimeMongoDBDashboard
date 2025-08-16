using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeMongoDashboard.Application.Interfaces;
using RealTimeMongoDashboard.Application.Models;
using System.Text.Json;

namespace RealTimeMongoDashboard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // secure all endpoints
public sealed class DataController : ControllerBase
{
    private readonly ICollectionService _svc;
    public DataController(ICollectionService svc) => _svc = svc;

    [HttpGet("{collection}")]
    public async Task<ActionResult<PagedResult<object>>> GetPage(
        string collection,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? filter = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
        => Ok(await _svc.GetPageAsync(collection, page, pageSize, filter, sort, ct));

    [HttpGet("{collection}/{id}")]
    public async Task<ActionResult<object>> GetById(string collection, string id, CancellationToken ct)
    {
        var result = await _svc.GetByIdAsync(collection, id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{collection}")]
    public async Task<ActionResult<string>> Create(string collection, [FromBody] JsonElement payload, CancellationToken ct)
    {
        var id = await _svc.InsertAsync(collection, payload, ct);
        return CreatedAtAction(nameof(GetById), new { collection, id }, new { id });
    }

    [HttpPut("{collection}/{id}")]
    public async Task<IActionResult> Update(string collection, string id, [FromBody] JsonElement payload, CancellationToken ct)
        => await _svc.UpdateAsync(collection, id, payload, ct) ? NoContent() : NotFound();

    [HttpDelete("{collection}/{id}")]
    public async Task<IActionResult> Delete(string collection, string id, CancellationToken ct)
        => await _svc.DeleteAsync(collection, id, ct) ? NoContent() : NotFound();
}
