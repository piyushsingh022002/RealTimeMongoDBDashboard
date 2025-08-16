namespace RealTimeMongoDashboard.Application.Interfaces;

public interface ICollectionService
{
    Task<PagedResult<object>> GetPageAsync(string collection, int page, int pageSize, string? filterJson = null, string? sortJson = null, CancellationToken ct = default);
    Task<object?> GetByIdAsync(string collection, string id, CancellationToken ct = default);
    Task<string> InsertAsync(string collection, object document, CancellationToken ct = default);
    Task<bool> UpdateAsync(string collection, string id, object document, CancellationToken ct = default);
    Task<bool> DeleteAsync(string collection, string id, CancellationToken ct = default);
}
