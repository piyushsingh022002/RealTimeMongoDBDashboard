using Application.DTOs;
using MongoDB.Bson;

namespace Application.Interfaces;

public interface IDynamicCollectionService
{
    Task<PagedResult<BsonDocument>> GetAllAsync(string collection, int page, int pageSize, CancellationToken ct);
    Task<BsonDocument> InsertAsync(string collection, BsonDocument doc, CancellationToken ct);
    Task<BsonDocument?> UpdateAsync(string collection, string id, BsonDocument patch, CancellationToken ct);
    Task<bool> DeleteAsync(string collection, string id, CancellationToken ct);
}