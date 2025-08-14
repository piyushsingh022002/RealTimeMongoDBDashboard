using Application.DTOs;
using Application.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Infrastructure.Persistence;

namespace Infrastructure.Services;

public sealed class DynamicCollectionService : IDynamicCollectionService
{
    private readonly MongoClientFactory _factory;

    public DynamicCollectionService(MongoClientFactory factory)
        => _factory = factory;

    public async Task<PagedResult<BsonDocument>> GetAllAsync(string collection, int page, int pageSize, CancellationToken ct)
    {
        var col = _factory.GetCollection(collection);
        var filter = Builders<BsonDocument>.Filter.Empty;
        var total = await col.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await col.Find(filter)
                             .Skip((page - 1) * pageSize)
                             .Limit(pageSize)
                             .ToListAsync(ct);
        return new PagedResult<BsonDocument>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<BsonDocument> InsertAsync(string collection, BsonDocument doc, CancellationToken ct)
    {
        var col = _factory.GetCollection(collection);
        doc["createdAt"] = DateTime.UtcNow;
        await col.InsertOneAsync(doc, cancellationToken: ct);
        return doc;
    }

    public async Task<BsonDocument?> UpdateAsync(string collection, string id, BsonDocument patch, CancellationToken ct)
    {
        var col = _factory.GetCollection(collection);
        if (!ObjectId.TryParse(id, out var oid)) return null;

        var update = new BsonDocument("$set", patch.Add("updatedAt", DateTime.UtcNow));
        var result = await col.FindOneAndUpdateAsync(
            Builders<BsonDocument>.Filter.Eq("_id", oid),
            new BsonDocumentUpdateDefinition<BsonDocument>(update),
            new FindOneAndUpdateOptions<BsonDocument> { ReturnDocument = ReturnDocument.After },
            ct);
        return result;
    }

    public async Task<bool> DeleteAsync(string collection, string id, CancellationToken ct)
    {
        var col = _factory.GetCollection(collection);
        if (!ObjectId.TryParse(id, out var oid)) return false;
        var res = await col.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", oid), ct);
        return res.DeletedCount > 0;
    }
}